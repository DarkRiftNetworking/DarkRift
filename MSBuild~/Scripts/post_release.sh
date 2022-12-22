#!/usr/bin/env bash

# Set exit on failure
set -e 
set -o pipefail

# Load arguments
directory=$1

if [[ -z "$directory" ]] || [[ ! -d "$directory" ]]
then
    echo "Specified releases directory is not a directory."
    exit 1
fi

version=$(python ./scripts/get_version.py)

free_package="$directory/DarkRift $version Free.unitypackage"
pro_package="$directory/DarkRift $version Pro.unitypackage"

free_framework_build="$directory/DarkRift $version Free - Framework.zip"
pro_framework_build="$directory/DarkRift $version Pro - Framework.zip"
pro_core20_package="$directory/DarkRift $version Pro - netcoreapp2.0.zip"
pro_core31_package="$directory/DarkRift $version Pro - netcoreapp3.1.zip"
pro_net50_package="$directory/DarkRift $version Pro - net5.0.zip"
pro_net60_package="$directory/DarkRift $version Pro - net6.0.zip"

# Copy relevant files to releases archive
echo "Copying zip folders as $version"
cp "./Build/Free/net4.0/DarkRift Server.zip" "$free_framework_build"
cp "./Build/Pro/net4.0/DarkRift Server.zip" "$pro_framework_build"
cp "./Build/Pro/netcoreapp2.0/DarkRift Server.zip" "$pro_core20_package"
cp "./Build/Pro/netcoreapp3.1/DarkRift Server.zip" "$pro_core31_package"
cp "./Build/Pro/net5.0/DarkRift Server.zip" "$pro_net50_package"
cp "./Build/Pro/net6.0/DarkRift Server.zip" "$pro_net60_package"

# Run docs build
echo "Building documentation"
docfx.exe ./DarkRift.Documentation/docfx.json

# Lines have been remove here because they were pointing to private stuff. This unfortunately includes Unity package building so it requires some work to make a new open build process and BUILDING.md tutorial for that