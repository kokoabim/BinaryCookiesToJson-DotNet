#!/opt/homebrew/bin/bash

set -eo pipefail

script_name="${0##*/}"
script_action="Publish to /opt/kokoabim/consoleapps"

yes=0
while getopts "hy" opt; do
    case $opt in
    h)
        echo "$script_action"
        echo "Usage: $script_name [-hy]" >&2
        exit 0
        ;;
    y) yes=1 ;;
    \?) exit 1 ;;
    esac
done
shift $(($OPTIND - 1))

function confirm_run() {
    if [[ ${yes:-false} == 1 ]]; then
        return
    fi

    read -p "${script_action}? [y/N] " -n 1
    [[ $REPLY == "" ]] && echo -en "\033[1A" >&2
    echo >&2
    [[ $REPLY =~ ^[Yy]$ ]]
}

if ! confirm_run; then
    echo "Canceled"
    exit 1
fi

echo "Publishing project"
rm -rf ./publish
dotnet publish -c Release -r osx-arm64 -o ./publish src/Kokoabim.BinaryCookiesToJson/Kokoabim.BinaryCookiesToJson.csproj
chown -R spencer:staff ./publish

echo "Copying published files"
mkdir -p /opt/kokoabim/consoleapps/bc2j
cp ./publish/* /opt/kokoabim/consoleapps/bc2j/
chown -R spencer:staff /opt/kokoabim/consoleapps/bc2j
