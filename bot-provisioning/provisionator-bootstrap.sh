#!/bin/bash -e
#
# Remove the following GUID if you do not wish for this script self-update:
# D6F71FB5-F2A7-4A62-86D3-10DFE08301CC
# https://github.com/xamarin/provisionator

function selfdir { (cd "$(dirname "$1")"; echo "$PWD"; ) }

selfdir=$(selfdir "$0")

channel="${PROVISIONATOR_CHANNEL:-latest}"

base_url="https://bosstoragemirror.azureedge.net/provisionator/664bd334021e3102cdef1af66c4fc9f1b2ecd2a21b47419e80d08da1f6c61c2a/${channel}"
latest_version_url="${base_url}/version"

archive_name="provisionator.osx.10.11-x64.zip"
archive_path="${selfdir}/${archive_name}"
archive_extract_path="${selfdir}/_provisionator"
archive_url="${base_url}/${archive_name}"
binary_path="${archive_extract_path}/provisionator"

set +e
latest_version="$(curl -fsL "${latest_version_url}")"
if [ $? != 0 ]; then
  echo "Unable to determine latest version from ${latest_version_url}"
  exit 1
fi
set -e

function update_in_place {
  echo "Downloading Provisionator $latest_version..."
  local progress_type="-s"
  tty -s && progress_type="-#"
  curl -f $progress_type -o "$archive_path" "$archive_url"
  rm -rf "$archive_extract_path"
  unzip -q -o -d "$archive_extract_path" "$archive_path"
  rm -f "$archive_path"
}

if [ -f "$binary_path" ]; then
  chmod +x "$binary_path"
  current_version="$("$binary_path" -version 2>&1 || true)"
  if [ "$latest_version" != "$current_version" ]; then
    update_in_place
  fi
else
  update_in_place
fi

exec "$binary_path" "$@"
