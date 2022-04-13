#!/usr/bin/env python
import sys

VERSION_PATTERN = r"^\d+\.\d+\.\d+$"


"""
Validate version number.
"""
def validate_version_number(version_number):
    import re
    return re.match(VERSION_PATTERN, version_number)


"""
Update version in .props file.
"""
def update_version_number(version_number):
    import xml.etree.ElementTree as ET
    xmldoc = ET.parse("./.props")
    root = xmldoc.getroot()

    root.find("PropertyGroup").find("Version").text = version_number

    xmldoc.write("./.props")

if validate_version_number(sys.argv[1]):
	update_version_number(sys.argv[1])
else:
    print("Version number does not follow allowed pattern.")
