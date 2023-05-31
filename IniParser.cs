using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IniParser
{
    public class IniConfig
    {
	Dictionary<string, Dictionary<string, string>> conf_map;

	public IniConfig()
	{
	    this.conf_map = new Dictionary<string, Dictionary<string, string>>();
	    this.AddSection("");
	}

	public IniConfig(string path)
	{
	    this.conf_map = new Dictionary<string, Dictionary<string, string>>();
	    this.AddSection("");
	    this.Parse(path);
	}

	public void Parse(string path)
	{
	    StreamReader file_reader = File.OpenText(path);
	    IniParser parser = new IniParser();

	    string line;
	    int line_num = 1;
	    while (null != (line = file_reader.ReadLine())) {
		parser.Parse(line, line_num);
		++line_num;
	    }

	    string curr_sec = "";
	    foreach (IniParserObject token in parser.tokens)
	    {
		switch (token.type)
		{
		    case INI_PARSER_TYPE.COMMENT:
			continue;
		    case INI_PARSER_TYPE.ERROR:
			Console.Error.WriteLine("Warning: Syntax Error on line {0}", token.line_num);
			continue;
		    case INI_PARSER_TYPE.SECTION:
			curr_sec = token.section_name;
			try {
			    this.AddSection(curr_sec);
			} catch (ArgumentException e) {
			    if (this.HasSection(curr_sec))
				Console.Error.WriteLine("Error: Duplicate Section \"{0}\" on line {1}",
							curr_sec,
				                        token.line_num);
			    throw e;
			}
			break;
		    case INI_PARSER_TYPE.KEY:
			try {
			    this.AddKey(curr_sec, token.key_val_pair.Item1, token.key_val_pair.Item2);
			} catch (ArgumentException e) {
			    if (this.HasKey(curr_sec, token.key_val_pair.Item1))
				Console.Error.WriteLine("Error: Duplicate Key \"{0}\" in Section \"{1}\" on line {2}",
						  token.key_val_pair.Item1,
						  curr_sec,
				                  token.line_num);
			    throw e;
			}
			break;
		}
	    }
	}

	public void Write(string path)
	{
	    var fs = File.Open(path, FileMode.Create);
	    byte[] conf_bytes = new UTF8Encoding(true).GetBytes(this.ToString());

	    fs.Write(conf_bytes, 0, conf_bytes.Length);
	}

	public void AddSection(string sec)
	{
	    conf_map.Add(sec, new Dictionary<string, string>());
	}

	public void AddKey(string sec, string key, string val)
	{
	    conf_map[sec].Add(key, val);
	}

	public void SetKey(string sec, string key, string new_val)
	{
	    this[sec, key] = new_val;
	}

	public string GetKey(string sec, string key)
	{
	    return this[sec, key];
	}

	public string this[string sec, string key]
	{
	    get => conf_map[sec][key];
	    set => conf_map[sec][key] = value;
	}

	public string[] GetAllSections()
	{
	    return conf_map.Keys.ToArray();
	}

	public string[] GetAllKeys(string sec)
	{
	    return conf_map[sec].Keys.ToArray();
	}

	public bool HasSection(string sec)
	{
	    return conf_map.ContainsKey(sec);
	}

	public bool HasKey(string sec, string key)
	{
	    return conf_map[sec].ContainsKey(key);
	}

	public void DelSection(string sec)
	{
	    if (sec != "")
		conf_map.Remove(sec);
	    else
		foreach (string key in conf_map[""].Keys)
		    conf_map[""].Remove(key);
	}

	public void DelKey(string sec, string key)
	{
	    conf_map[sec].Remove(key);
	}

	public override string ToString()
	{
	    string str = "";

	    foreach (string sec in conf_map.Keys)
	    {
		if (sec != "")
		    str += '[' + sec + ']' + '\n';

		foreach (string key in conf_map[sec].Keys)
		    str += key + '=' + conf_map[sec][key] + '\n';
	    }
	    return str;
	}
    }

    internal class IniParser
    {
	internal List<IniParserObject> tokens;

	internal IniParser()
	{
	    this.tokens = new List<IniParserObject>();
	}

	internal void Parse(string line, int line_num)
	{
	    string is_section = @"^\s*\[\s*[^][;=]+\s*\]\s*(;.*)?$";
	    string is_key = @"^\s*[^][;=]+\s*=\s*[^][;=]+\s*(;.*)?$";
	    string is_comment = @"^\s*;.*$";
	    string is_empty = @"^\s*$";
	    string match_value = @"(?=\S)[^][;=]+(?<=\S)";

	    if (Regex.IsMatch(line, is_section))
	    {
		this.tokens.Add(new IniParserObject(line_num, Regex.Match(line, match_value).Value));
	    }

	    else if (Regex.IsMatch(line, is_key))
	    {
		var match = Regex.Match(line, match_value);
		string key = match.Value;
		match = match.NextMatch();
		this.tokens.Add(new IniParserObject(line_num, key, match.Value));
	    }

	    else if (Regex.IsMatch(line, is_comment))
	    {
		this.tokens.Add(new IniParserObject(line_num, INI_PARSER_TYPE.COMMENT));
	    }

	    else if (Regex.IsMatch(line, is_empty))
	    {
		return;
	    }
	    else
	    {
		this.tokens.Add(new IniParserObject(line_num, INI_PARSER_TYPE.ERROR));
	    }
	}
    }
    
    internal struct IniParserObject
    {
	internal int line_num { get; }
	internal INI_PARSER_TYPE type { get; }
	internal string section_name { get; }
	internal (string, string) key_val_pair { get; }

	internal IniParserObject(int line, INI_PARSER_TYPE type)
	{
	    this.line_num = line;
	    this.type = type;
	    this.section_name = null;
	    this.key_val_pair = (null, null);
	}

	internal IniParserObject(int line, string section_name)
	{
	    this.line_num = line;
	    this.type = INI_PARSER_TYPE.SECTION;
	    this.section_name = section_name;
	    this.key_val_pair = (null, null);
	}

	internal IniParserObject(int line, string key, string val)
	{
	    this.line_num = line;
	    this.type = INI_PARSER_TYPE.KEY;
	    this.section_name = null;
	    this.key_val_pair = (key, val);
	}
    }

    internal enum INI_PARSER_TYPE { SECTION, KEY, COMMENT, ERROR }
}
