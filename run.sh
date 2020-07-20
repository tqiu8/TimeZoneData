#!/usr/bin/env bash
set -e
set -u

input_dir="obj/data/input"
output_dir="obj/data/output"
data_name="dotnet.timezones.blat"
type="timezone"

while (("$#));
do 
    key="${1}"

    case ${key} in
    -i|--input_dir)
        input_dir="$2"
        shift 2
        ;;
    -t|--type)
        type="$2"
        shift 2
        ;;
    -d|-data_name)
        data_name="$2"
        shift 2
        ;;
    *)
        shift
        ;;
    esac
done

if [[ -d "obj/data" ]]
then
    rm -rf "obj/data"
fi

mkdir -p "$input_dir"
mkdir "$output_dir"

if [ type == "timezone" ]; then
    curl -L https://data.iana.org/time-zones/tzdata-latest.tar.gz -o "$input_dir/tzdata.tar.gz"
    tar xvzf "$input_dir/tzdata.tar.gz" -C "$input_dir"

    files=("africa"  "antarctica"  "asia"  "australasia"  "etcetera"  "europe"  "northamerica"  "southamerica" "backward")

    for file in "${files[@]}"
    do
        zic -d "$output_dir" "$input_dir/$file"
    done
fi

dotnet run $type $input_dir $data_name