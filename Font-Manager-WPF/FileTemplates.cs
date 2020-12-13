namespace CSGO_Font_Manager
{
    internal class FileTemplates
    {
        public static string fonts_conf(string fontname, string fontext, string fontfile, string pixelSize) =>
            "<?xml version='1.0'?>\n" +
            "<!DOCTYPE fontconfig SYSTEM 'fonts.dtd'>\n" +
            "<fontconfig>\n" +
            "\n" +
            "	<!-- Choose an OS Rendering Style.  This will determine B/W, grayscale,\n" +
            "	     or subpixel antialising and slight, full or no hinting and replacements (if set in next option) -->\n" +
            "	<!-- Style should also be set in the infinality-settings.sh file, ususally in /etc/profile.d/ -->\n" +
            "\n" +
            "	<!-- Choose one of these options:\n" +
            "		Infinality      - subpixel AA, minimal replacements/tweaks, sans=Arial\n" +
            "		Windows 7       - subpixel AA, sans=Arial\n" +
            "		Windows XP      - subpixel AA, sans=Arial\n" +
            "		Windows 98      - B/W full hinting on TT fonts, grayscale AA for others, sans=Arial\n" +
            "		OSX             - Slight hinting, subpixel AA, sans=Helvetica Neue\n" +
            "		OSX2            - No hinting, subpixel AA, sans=Helvetica Neue\n" +
            "		Linux           - subpixel AA, sans=DejaVu Sans\n" +
            "\n" +
            "	=== Recommended Setup ===\n" +
            "	Run ./infctl.sh script located in the current directory to set the style.\n" +
            "	\n" +
            "	# ./infctl.sh setstyle\n" +
            "	\n" +
            "	=== Manual Setup ===\n" +
            "	See the infinality/styles.conf.avail/ directory for all options.  To enable \n" +
            "	a different style, remove the symlink \"conf.d\" and link to another style:\n" +
            "	\n" +
            "	# rm conf.d\n" +
            "	# ln -s styles.conf.avail/win7 conf.d\n" +
            "	-->\n" +
            "\n" +
            "	<dir>WINDOWSFONTDIR</dir>\n" +
            "	<dir>~/.fonts</dir>\n" +
            "	<dir>/usr/share/fonts</dir>\n" +
            "	<dir>/usr/local/share/fonts</dir>\n" +
            "	<dir prefix=\"xdg\">fonts</dir>\n" +
            "\n" +
            "	<!-- A fontpattern is a font file name, not a font name.  Be aware of filenames across all platforms! -->\n" +
            "	<fontpattern>" + fontname + "</fontpattern>\n" +
            "	<fontpattern>" + fontext + "</fontpattern>\n" +
            "	<fontpattern>" + fontfile + "</fontpattern>\n" +
            "	\n" +
            "	<cachedir>WINDOWSTEMPDIR_FONTCONFIG_CACHE</cachedir>\n" +
            "	<cachedir>~/.fontconfig</cachedir>\n" +
            "\n" +
            "	<!-- Uncomment this to reject all bitmap fonts -->\n" +
            "	<!-- Make sure to run this as root if having problems:  fc-cache -f -->\n" +
            "	<!--\n" +
            "	<selectfont>\n" +
            "		<rejectfont>\n" +
            "			<pattern>\n" +
            "				<patelt name=\"scalable\" >\n" +
            "					<bool>false</bool>\n" +
            "				</patelt>\n" +
            "			</pattern>\n" +
            "		</rejectfont>\n" +
            "	</selectfont>\n" +
            "	-->\n" +
            "\n" +
            "	<!-- The Stratum2 Monodigit fonts just supply the monospaced digits -->\n" +
            "	<!-- All other characters should come from ordinary Stratum2 -->\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Stratum2 Bold Monodigit</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>Stratum2 Bold</string>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Stratum2 Regular Monodigit</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Stratum2 only contains a subset of the Vietnamese alphabet. -->\n" +
            "	<!-- So when language is set to Vietnamese, replace Stratum with Noto. -->\n" +
            "	<!-- Adjust size due to the Ascent value for Noto being significantly larger than Stratum.. -->\n" +
            "	<match>\n" +
            "		<test name=\"lang\">\n" +
            "			<string>vi-vn</string>\n" +
            "		</test>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"assign\" binding=\"same\">\n" +
            "			<string>notosans</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>0.9</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- More Vietnamese... -->\n" +
            "	<!-- In some cases (hud health, ammo, money) we want to force Stratum to be used. -->\n" +
            "	<match>\n" +
            "		<test name=\"lang\">\n" +
            "			<string>vi-vn</string>\n" +
            "		</test>\n" +
            "		<test name=\"family\">\n" +
            "			<string>ForceStratum2</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"assign\" binding=\"same\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Fallback font sizes. -->\n" +
            "	<!-- If we request Stratum, but end up with Arial, reduce the pixelsize because Arial glyphs are larger than Stratum. -->\n" +
            "	<match target=\"font\">\n" +
            "		<test name=\"family\" target=\"pattern\" compare=\"contains\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</test>\n" +
            "		<test name=\"family\" target=\"font\" compare=\"contains\">\n" +
            "			<string>Arial</string>\n" +
            "		</test>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>0.9</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- If we request Stratum, but end up with Noto, reduce the pixelsize. -->\n" +
            "	<!-- This fixes alignment issues due to the Ascent value for Noto being significantly larger than Stratum. -->\n" +
            "	<match target=\"font\">\n" +
            "		<test name=\"family\" target=\"pattern\" compare=\"contains\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</test>\n" +
            "		<test name=\"family\" target=\"font\" compare=\"contains\">\n" +
            "			<string>Noto</string>\n" +
            "		</test>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>0.9</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            " 	<!-- Stratum contains a set of arrow symbols in place of certain greek/mathematical characters - presumably for some historical reason, possibly used by VGUI somewhere?. -->\n" +
            " 	<!-- For panorama these Stratum characters should be ignored and picked up from a fallback font instead. -->\n" +
            "	<match target=\"scan\">\n" +
            "		<test name=\"family\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</test>\n" +
            "		<edit name=\"charset\" mode=\"assign\">\n" +
            "			<minus>\n" +
            "				<name>charset</name>\n" +
            "				<charset>\n" +
            "					<int>0x03A9</int> <!-- greek omega -->\n" +
            "					<int>0x03C0</int> <!-- greek pi -->\n" +
            "					<int>0x2202</int> <!-- partial diff -->\n" +
            "					<int>0x2206</int> <!-- delta -->\n" +
            "					<int>0x220F</int> <!-- product -->\n" +
            "					<int>0x2211</int> <!-- sum -->\n" +
            "					<int>0x2212</int> <!-- minus -->\n" +
            "					<int>0x221A</int> <!-- square root -->\n" +
            "					<int>0x221E</int> <!-- infinity -->\n" +
            "					<int>0x222B</int> <!-- integral -->\n" +
            "					<int>0x2248</int> <!-- approxequal -->\n" +
            "					<int>0x2260</int> <!-- notequal -->\n" +
            "					<int>0x2264</int> <!-- lessequal -->\n" +
            "					<int>0x2265</int> <!-- greaterequal -->\n" +
            "					<int>0x25CA</int> <!-- lozenge -->\n" +
            "				</charset>\n" +
            "			</minus>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Ban Type-1 fonts because they render poorly --> \n" +
            "	<!-- Comment this out to allow all Type 1 fonts -->\n" +
            "	<selectfont> \n" +
            "		<rejectfont> \n" +
            "			<pattern> \n" +
            "				<patelt name=\"fontformat\" > \n" +
            "					<string>Type 1</string> \n" +
            "				</patelt> \n" +
            "			</pattern> \n" +
            "		</rejectfont> \n" +
            "	</selectfont> \n" +
            "\n" +
            "	<!-- Globally use embedded bitmaps in fonts like Calibri? -->\n" +
            "	<match target=\"font\" >\n" +
            "		<edit name=\"embeddedbitmap\" mode=\"assign\">\n" +
            "			<bool>false</bool>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Substitute truetype fonts in place of bitmap ones? -->\n" +
            "	<match target=\"pattern\" >\n" +
            "		<edit name=\"prefer_outline\" mode=\"assign\">\n" +
            "			<bool>true</bool>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Do font substitutions for the set style? -->\n" +
            "	<!-- NOTE: Custom substitutions in 42-repl-global.conf will still be done -->\n" +
            "	<!-- NOTE: Corrective substitutions will still be done -->\n" +
            "	<match target=\"pattern\" >\n" +
            "		<edit name=\"do_substitutions\" mode=\"assign\">\n" +
            "			<bool>true</bool>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Make (some) monospace/coding TTF fonts render as bitmaps? -->\n" +
            "	<!-- courier new, andale mono, monaco, etc. -->\n" +
            "	<match target=\"pattern\" >\n" +
            "		<edit name=\"bitmap_monospace\" mode=\"assign\">\n" +
            "			<bool>false</bool>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Force autohint always -->\n" +
            "	<!-- Useful for debugging and for free software purists -->\n" +
            "	<match target=\"font\">\n" +
            "		<edit name=\"force_autohint\" mode=\"assign\">\n" +
            "			<bool>false</bool>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Set DPI.  dpi should be set in ~/.Xresources to 96 -->\n" +
            "	<!-- Setting to 72 here makes the px to pt conversions work better (Chrome) -->\n" +
            "	<!-- Some may need to set this to 96 though -->\n" +
            "	<match target=\"pattern\">\n" +
            "		<edit name=\"dpi\" mode=\"assign\">\n" +
            "			<double>96</double>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "	\n" +
            "	<!-- Use Qt subpixel positioning on autohinted fonts? -->\n" +
            "	<!-- This only applies to Qt and autohinted fonts. Qt determines subpixel positioning based on hintslight vs. hintfull, -->\n" +
            "	<!--   however infinality patches force slight hinting inside freetype, so this essentially just fakes out Qt. -->\n" +
            "	<!-- Should only be set to true if you are not doing any stem alignment or fitting in environment variables -->\n" +
            "	<match target=\"pattern\" >\n" +
            "		<edit name=\"qt_use_subpixel_positioning\" mode=\"assign\">\n" +
            "			<bool>false</bool>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- Run infctl.sh or change the symlink in current directory instead of modifying this -->\n" +
             "	<include>conf.d</include>\n" + // This may not be needed
            "	\n" +
            "	<!-- Custom fonts -->\n" +
            "	<!-- Edit every occurency with your font name (NOT the font file name) -->\n" +
            "	\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Stratum2</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>" + fontname + "</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>" + pixelSize + "</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "	\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Stratum2 Bold</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>" + fontname + "</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>" + pixelSize + "</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "	\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Arial</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>" + fontname + "</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>" + pixelSize + "</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "	\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Times New Roman</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>" + fontname + "</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>" + pixelSize + "</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "	\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>Courier New</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>" + fontname + "</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>" + pixelSize + "</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "\n" +
            "	<!-- And here's the thing... -->\n" +
            "	<!-- CSGO devs decided to fallback to \"notosans\" on characters not supplied with \"Stratum2\" - the font we're trying to replace -->\n" +
            "	<!-- \"notosans\" or \"Noto\" is used i.e. on Vietnamese characters - but also on some labels that should be using \"Stratum2\" or even Arial -->\n" +
            "	<!-- I can't do much about it right now. If you're Vietnamese or something, just delete this <match> closure. -->\n" +
            "	<!-- Some labels (i.e. icon tooltips in menu) won't be using your custom font -->\n" +
            "	<match>\n" +
            "		<test name=\"family\">\n" +
            "			<string>notosans</string>\n" +
            "		</test>\n" +
            "		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
            "			<string>" + fontname + "</string>\n" +
            "		</edit>\n" +
            "		<edit name=\"pixelsize\" mode=\"assign\">\n" +
            "			<times>\n" +
            "				<name>pixelsize</name>\n" +
            "				<double>" + pixelSize + "</double>\n" +
            "			</times>\n" +
            "		</edit>\n" +
            "	</match>\n" +
            "	\n" +
            "</fontconfig>";

        public static string fonts_conf_backup() =>
"<?xml version='1.0'?>\n" +
"<!DOCTYPE fontconfig SYSTEM 'fonts.dtd'>\n" +
"<fontconfig>\n" +
"\n" +
"	<!-- Choose an OS Rendering Style.  This will determine B/W, grayscale,\n" +
"	     or subpixel antialising and slight, full or no hinting and replacements (if set in next option) -->\n" +
"	<!-- Style should also be set in the infinality-settings.sh file, ususally in /etc/profile.d/ -->\n" +
"\n" +
"	<!-- Choose one of these options:\n" +
"		Infinality      - subpixel AA, minimal replacements/tweaks, sans=Arial\n" +
"		Windows 7       - subpixel AA, sans=Arial\n" +
"		Windows XP      - subpixel AA, sans=Arial\n" +
"		Windows 98      - B/W full hinting on TT fonts, grayscale AA for others, sans=Arial\n" +
"		OSX             - Slight hinting, subpixel AA, sans=Helvetica Neue\n" +
"		OSX2            - No hinting, subpixel AA, sans=Helvetica Neue\n" +
"		Linux           - subpixel AA, sans=DejaVu Sans\n" +
"\n" +
"	=== Recommended Setup ===\n" +
"	Run ./infctl.sh script located in the current directory to set the style.\n" +
"	\n" +
"	# ./infctl.sh setstyle\n" +
"	\n" +
"	=== Manual Setup ===\n" +
"	See the infinality/styles.conf.avail/ directory for all options.  To enable \n" +
"	a different style, remove the symlink \"conf.d\" and link to another style:\n" +
"	\n" +
"	# rm conf.d\n" +
"	# ln -s styles.conf.avail/win7 conf.d\n" +
"	-->\n" +
"\n" +
"	<dir>WINDOWSFONTDIR</dir>\n" +
"	<dir>~/.fonts</dir>\n" +
"	<dir>/usr/share/fonts</dir>\n" +
"	<dir>/usr/local/share/fonts</dir>\n" +
"	<dir prefix=\"xdg\">fonts</dir>\n" +
"\n" +
"	<!-- A fontpattern is a font file name, not a font name.  Be aware of filenames across all platforms! -->\n" +
"	<fontpattern>Arial</fontpattern>\n" +
"	<fontpattern>.vfont</fontpattern>\n" +
"	<fontpattern>notosans</fontpattern>\n" +
"	<fontpattern>notoserif</fontpattern>\n" +
"	<fontpattern>notomono-regular</fontpattern>\n" +
"	\n" +
"	<cachedir>WINDOWSTEMPDIR_FONTCONFIG_CACHE</cachedir>\n" +
"	<cachedir>~/.fontconfig</cachedir>\n" +
"\n" +
"	<!-- Uncomment this to reject all bitmap fonts -->\n" +
"	<!-- Make sure to run this as root if having problems:  fc-cache -f -->\n" +
"	<!--\n" +
"	<selectfont>\n" +
"		<rejectfont>\n" +
"			<pattern>\n" +
"				<patelt name=\"scalable\" >\n" +
"					<bool>false</bool>\n" +
"				</patelt>\n" +
"			</pattern>\n" +
"		</rejectfont>\n" +
"	</selectfont>\n" +
"	-->\n" +
"\n" +
"	<!-- The Stratum2 Monodigit fonts just supply the monospaced digits -->\n" +
"	<!-- All other characters should come from ordinary Stratum2 -->\n" +
"	<match>\n" +
"		<test name=\"family\">\n" +
"			<string>Stratum2 Bold Monodigit</string>\n" +
"		</test>\n" +
"		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
"			<string>Stratum2 Bold</string>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<match>\n" +
"		<test name=\"family\">\n" +
"			<string>Stratum2 Regular Monodigit</string>\n" +
"		</test>\n" +
"		<edit name=\"family\" mode=\"append\" binding=\"strong\">\n" +
"			<string>Stratum2</string>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Stratum2 only contains a subset of the Vietnamese alphabet. -->\n" +
"	<!-- So when language is set to Vietnamese, replace Stratum with Noto. -->\n" +
"	<!-- Adjust size due to the Ascent value for Noto being significantly larger than Stratum.. -->\n" +
"	<match>\n" +
"		<test name=\"lang\">\n" +
"			<string>vi-vn</string>\n" +
"		</test>\n" +
"		<test name=\"family\">\n" +
"			<string>Stratum2</string>\n" +
"		</test>\n" +
"		<edit name=\"family\" mode=\"assign\" binding=\"same\">\n" +
"			<string>notosans</string>\n" +
"		</edit>\n" +
"		<edit name=\"pixelsize\" mode=\"assign\">\n" +
"			<times>\n" +
"				<name>pixelsize</name>\n" +
"				<double>0.9</double>\n" +
"			</times>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- More Vietnamese... -->\n" +
"	<!-- In some cases (hud health, ammo, money) we want to force Stratum to be used. -->\n" +
"	<match>\n" +
"		<test name=\"lang\">\n" +
"			<string>vi-vn</string>\n" +
"		</test>\n" +
"		<test name=\"family\">\n" +
"			<string>ForceStratum2</string>\n" +
"		</test>\n" +
"		<edit name=\"family\" mode=\"assign\" binding=\"same\">\n" +
"			<string>Stratum2</string>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Fallback font sizes. -->\n" +
"	<!-- If we request Stratum, but end up with Arial, reduce the pixelsize because Arial glyphs are larger than Stratum. -->\n" +
"	<match target=\"font\">\n" +
"		<test name=\"family\" target=\"pattern\" compare=\"contains\">\n" +
"			<string>Stratum2</string>\n" +
"		</test>\n" +
"		<test name=\"family\" target=\"font\" compare=\"contains\">\n" +
"			<string>Arial</string>\n" +
"		</test>\n" +
"		<edit name=\"pixelsize\" mode=\"assign\">\n" +
"			<times>\n" +
"				<name>pixelsize</name>\n" +
"				<double>0.9</double>\n" +
"			</times>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- If we request Stratum, but end up with Noto, reduce the pixelsize. -->\n" +
"	<!-- This fixes alignment issues due to the Ascent value for Noto being significantly larger than Stratum. -->\n" +
"	<match target=\"font\">\n" +
"		<test name=\"family\" target=\"pattern\" compare=\"contains\">\n" +
"			<string>Stratum2</string>\n" +
"		</test>\n" +
"		<test name=\"family\" target=\"font\" compare=\"contains\">\n" +
"			<string>Noto</string>\n" +
"		</test>\n" +
"		<edit name=\"pixelsize\" mode=\"assign\">\n" +
"			<times>\n" +
"				<name>pixelsize</name>\n" +
"				<double>0.9</double>\n" +
"			</times>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
" 	<!-- Stratum contains a set of arrow symbols in place of certain greek/mathematical characters - presumably for some historical reason, possibly used by VGUI somewhere?. -->\n" +
" 	<!-- For panorama these Stratum characters should be ignored and picked up from a fallback font instead. -->\n" +
"	<match target=\"scan\">\n" +
"		<test name=\"family\">\n" +
"			<string>Stratum2</string>\n" +
"		</test>\n" +
"		<edit name=\"charset\" mode=\"assign\">\n" +
"			<minus>\n" +
"				<name>charset</name>\n" +
"				<charset>\n" +
"					<int>0x03A9</int> <!-- greek omega -->\n" +
"					<int>0x03C0</int> <!-- greek pi -->\n" +
"					<int>0x2202</int> <!-- partial diff -->\n" +
"					<int>0x2206</int> <!-- delta -->\n" +
"					<int>0x220F</int> <!-- product -->\n" +
"					<int>0x2211</int> <!-- sum -->\n" +
"					<int>0x2212</int> <!-- minus -->\n" +
"					<int>0x221A</int> <!-- square root -->\n" +
"					<int>0x221E</int> <!-- infinity -->\n" +
"					<int>0x222B</int> <!-- integral -->\n" +
"					<int>0x2248</int> <!-- approxequal -->\n" +
"					<int>0x2260</int> <!-- notequal -->\n" +
"					<int>0x2264</int> <!-- lessequal -->\n" +
"					<int>0x2265</int> <!-- greaterequal -->\n" +
"					<int>0x25CA</int> <!-- lozenge -->\n" +
"				</charset>\n" +
"			</minus>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Ban Type-1 fonts because they render poorly --> \n" +
"	<!-- Comment this out to allow all Type 1 fonts -->\n" +
"	<selectfont> \n" +
"		<rejectfont> \n" +
"			<pattern> \n" +
"				<patelt name=\"fontformat\" > \n" +
"					<string>Type 1</string> \n" +
"				</patelt> \n" +
"			</pattern> \n" +
"		</rejectfont> \n" +
"	</selectfont> \n" +
"\n" +
"	<!-- Globally use embedded bitmaps in fonts like Calibri? -->\n" +
"	<match target=\"font\" >\n" +
"		<edit name=\"embeddedbitmap\" mode=\"assign\">\n" +
"			<bool>false</bool>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Substitute truetype fonts in place of bitmap ones? -->\n" +
"	<match target=\"pattern\" >\n" +
"		<edit name=\"prefer_outline\" mode=\"assign\">\n" +
"			<bool>true</bool>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Do font substitutions for the set style? -->\n" +
"	<!-- NOTE: Custom substitutions in 42-repl-global.conf will still be done -->\n" +
"	<!-- NOTE: Corrective substitutions will still be done -->\n" +
"	<match target=\"pattern\" >\n" +
"		<edit name=\"do_substitutions\" mode=\"assign\">\n" +
"			<bool>true</bool>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Make (some) monospace/coding TTF fonts render as bitmaps? -->\n" +
"	<!-- courier new, andale mono, monaco, etc. -->\n" +
"	<match target=\"pattern\" >\n" +
"		<edit name=\"bitmap_monospace\" mode=\"assign\">\n" +
"			<bool>false</bool>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Force autohint always -->\n" +
"	<!-- Useful for debugging and for free software purists -->\n" +
"	<match target=\"font\">\n" +
"		<edit name=\"force_autohint\" mode=\"assign\">\n" +
"			<bool>false</bool>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Set DPI.  dpi should be set in ~/.Xresources to 96 -->\n" +
"	<!-- Setting to 72 here makes the px to pt conversions work better (Chrome) -->\n" +
"	<!-- Some may need to set this to 96 though -->\n" +
"	<match target=\"pattern\">\n" +
"		<edit name=\"dpi\" mode=\"assign\">\n" +
"			<double>96</double>\n" +
"		</edit>\n" +
"	</match>\n" +
"	\n" +
"	<!-- Use Qt subpixel positioning on autohinted fonts? -->\n" +
"	<!-- This only applies to Qt and autohinted fonts. Qt determines subpixel positioning based on hintslight vs. hintfull, -->\n" +
"	<!--   however infinality patches force slight hinting inside freetype, so this essentially just fakes out Qt. -->\n" +
"	<!-- Should only be set to true if you are not doing any stem alignment or fitting in environment variables -->\n" +
"	<match target=\"pattern\" >\n" +
"		<edit name=\"qt_use_subpixel_positioning\" mode=\"assign\">\n" +
"			<bool>false</bool>\n" +
"		</edit>\n" +
"	</match>\n" +
"\n" +
"	<!-- Run infctl.sh or change the symlink in current directory instead of modifying this -->\n" +
"	<include>conf.d</include>\n" +
"\n" +
"</fontconfig>\n";
    }
}