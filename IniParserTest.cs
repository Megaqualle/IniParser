using System;

namespace IniParser
{
    class IniParserTest
    {
	static void Main()
	{
	    var conf = new IniConfig();
	    conf.AddSection("test");
	    conf.AddKey("", "key1", "val1");
	    conf.AddKey("test", "key2", "val2");

	    print_conf(conf, "Stage 1");

	    conf.DelKey("", "key1");
	    conf.SetKey("test", "key2", "val3");

	    print_conf(conf, "Stage 2");

	    conf.DelSection("test");

	    print_conf(conf, "Stage 3");

	    test_parse();
	}

	static void test_parse()
	{
	    var conf = new IniConfig(@"example.ini");
	    print_conf(conf, "Parse");
	}

	static void print_conf(IniConfig conf, string msg)
	{
	    Console.WriteLine("\n===============");
	    Console.WriteLine(msg);
	    Console.WriteLine("===============");
	    Console.Write("{0}", conf.ToString());
	    Console.WriteLine("===============");
	}
    }
}
