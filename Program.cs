// Decompiled with JetBrains decompiler
// Type: CanalCocina3.Program
// Assembly: CanalCocina3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9D302122-99E7-4337-B8A8-DD06FFD49964
// Assembly location: C:\Users\xavi\Downloads\CanalCocina3.dll

using System;
using System.Text.RegularExpressions;

namespace CanalCocina3
{
  public class Program : IComparable<Program>
  {
        

        public string Title = string.Empty;
    public string Desc = string.Empty;
    public string Category = string.Empty;
    public int Session = -1;
    public int Chapter = -1;
    public DateTime? Start;
    public DateTime? Stop;

    public Program()
    {

    }

    public Program(DateTime? start, DateTime? stop, string title, string desc, string category)
    {
      this.Start = start;
      this.Stop = stop;
      this.Title = title;
      this.Desc = desc;
      this.Category = category;
    }

    public string CleanXmlString(string s)
    {
      return Regex.Replace(Regex.Replace(s, "<>\"", ""), "'", " ");
    }

   

        public int CompareTo(Program otherProgram)
    {
      if (!this.Start.HasValue)
        return -1;
      if (!otherProgram.Start.HasValue)
        return 1;
      return DateTime.Compare(this.Start.Value, otherProgram.Start.Value);
    }
  }
}
