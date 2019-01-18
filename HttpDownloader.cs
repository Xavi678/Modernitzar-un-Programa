// Decompiled with JetBrains decompiler
// Type: CanalCocina3.HttpDownloader
// Assembly: CanalCocina3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9D302122-99E7-4337-B8A8-DD06FFD49964
// Assembly location: C:\Users\xavi\Downloads\CanalCocina3.dll

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CanalCocina3
{
  internal class HttpDownloader
  {
    private readonly string _referer;
    private readonly string _userAgent;

    public Encoding Encoding { get; set; }

    public WebHeaderCollection Headers { get; set; }

    public Uri Url { get; set; }

    public HttpDownloader(string url, string referer, string userAgent)
    {
      this.Url = new Uri(url);
      this._userAgent = userAgent;
      this._referer = referer;
      this.Encoding = (Encoding) null;
    }
    /// <summary>
    /// Afegeix als headers que obté de la sol·licitud, el header Accept Encoding amb els valors gzip,deflate, i dona a this.headers,this.url la repsosta del headers i la Uri
    /// </summary>
    /// <returns></returns>
    public string GetPage()
    {
      HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(this.Url);
      if (!string.IsNullOrEmpty(this._referer))
        httpWebRequest.Referer = this._referer;
      if (!string.IsNullOrEmpty(this._userAgent))
        httpWebRequest.UserAgent = this._userAgent;
      httpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
      using (HttpWebResponse response = (HttpWebResponse) httpWebRequest.GetResponse())
      {
        this.Headers = response.Headers;
        this.Url = response.ResponseUri;
        return this.ProcessContent(response);
      }
    }


    /// <summary>
    /// Per la repsosta obtinguda, descomprimeix el gzip i llegeix el codi html i el posa en un string amb la codificació corresponent
    /// </summary>
    /// <param name="response">La resposta que ha obtingut de la pàgina web</param>
    /// <returns>una string amb tot el codi html</returns>
    private string ProcessContent(HttpWebResponse response)
    {
      if (this.Encoding == null)
        this.SetEncodingFromHeader(response);
      Stream stream = response.GetResponseStream();
      if (response.ContentEncoding.ToLower().Contains("gzip"))
        stream = (Stream) new GZipStream(stream, CompressionMode.Decompress);
      else if (response.ContentEncoding.ToLower().Contains("deflate"))
        stream = (Stream) new DeflateStream(stream, CompressionMode.Decompress);
      MemoryStream memoryStream = new MemoryStream();
      byte[] buffer = new byte[4096];
      for (int count = stream.Read(buffer, 0, buffer.Length); count > 0; count = stream.Read(buffer, 0, buffer.Length))
        memoryStream.Write(buffer, 0, count);
      stream.Close();
      memoryStream.Position = 0L;
      using (StreamReader streamReader = new StreamReader((Stream) memoryStream, this.Encoding))
      {
        string html = streamReader.ReadToEnd().Trim();
        return this.CheckMetaCharSetAndReEncode((Stream) memoryStream, html);
      }
    }

/// <summary>
/// Obté la resposta de la web i si la codificació no es correcta ho arregla
/// </summary>
/// <param name="response"></param>
    private void SetEncodingFromHeader(HttpWebResponse response)
    {
      string name = (string) null;
      if (string.IsNullOrEmpty(response.CharacterSet))
      {
        System.Text.RegularExpressions.Match match = Regex.Match(response.ContentType, ";\\s*charset\\s*=\\s*(?<charset>.*)", RegexOptions.IgnoreCase);
        if (match.Success)
          name = match.Groups["charset"].Value.Trim('\'', '"');
      }
      else
        name = response.CharacterSet;
      if (string.IsNullOrEmpty(name))
        return;
      try
      {
        this.Encoding = Encoding.GetEncoding(name);
      }
      catch (ArgumentException ex)
      {
      }
    }

    /// <summary>
    /// Si obté una codificació no desitjada, tornar a codificar el codi
    /// </summary>
    /// <param name="memStream">Seqüencia de bytes</param>
    /// <param name="html">el codi html de la pàgina web</param>
    /// <returns>torna el codi html codificat correctament</returns>
    private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
    {
      System.Text.RegularExpressions.Match match = new Regex("<meta\\s+.*?charset\\s*=\\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(html);
      if (match.Success)
      {
        string name = match.Groups["charset"].Value.ToLower() ?? "iso-8859-1";
        if (name == "unicode" || name == "utf-16")
          name = "utf-8";
        try
        {
          Encoding encoding = Encoding.GetEncoding(name);
          if (this.Encoding != encoding)
          {
            memStream.Position = 0L;
            StreamReader streamReader = new StreamReader(memStream, encoding);
            html = streamReader.ReadToEnd().Trim();
            streamReader.Close();
          }
        }
        catch (ArgumentException ex)
        {
        }
      }
      return html;
    }
  }
}
