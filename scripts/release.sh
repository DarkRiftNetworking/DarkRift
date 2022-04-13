#!/usr/bin/env bash

# Set exit on failure
set -e 
set -o pipefail

# Load arguments
version=$1

if ! [[ $version =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]
then
    echo "Version number does not follow allowed pattern."
    exit 1
fi

if ! git diff-index --quiet HEAD --;
then
    echo "You have uncommitted changes in your project. Commit the changes or reset them to release."
    exit 1
fi

echo "Generating changelog"
echo
echo "DarkRift $version - $(date -I)"
git log $(git describe --tags --abbrev=0)..HEAD --format='- %s' | grep -vF "[changelog.skip]" | grep -v "^Merge"
echo

# Run release sequence
echo "Updating .props to version $version"
python ./scripts/update_props.py $1
git add .props

echo "Committing changes"
git commit -m "Update version number to $version"

echo "Adding tag V$version"
git tag V$version

echo "Pushing to remote server"
git push
git push --tags

echo
echo "Now:"
echo "1. Rebuild Free"
echo "2. Upload to the asset store"
echo "3. Export to releases folder"
echo
echo "4. Rebuild Pro"
echo "5. Upload to the asset store"
echo "6. Export to releases folder"
echo
echo "7. Run ./scripts/post_release.sh"
