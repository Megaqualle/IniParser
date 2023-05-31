include config.mk

default: IniParser.dll

IniParser.dll: IniParser.cs
	$(CS) -target:library $^

IniParserTest.exe: IniParserTest.cs IniParser.dll
	$(CS) -r:IniParser.dll $<

test: IniParserTest.exe
	mono ./$<

install: IniParser.dll
	install -c IniParser.dll $(PREFIX)/lib

uninstall:
	$(RM) $(PREFIX)/lib/IniParser.dll

clean:
	$(RM) IniParser.dll \
		IniParserTest.exe
