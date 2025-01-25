import hashlib
import os
import yaml
import json


def generate():
    with open('musicpath.yaml') as yamlfile:
        yamldict = yaml.safe_load(yamlfile)

    musicPath = yamldict['musicPath']

    # go to musicPath, walk through every dir+subdir and catalog relative path
    # plus file hash. store in dictionary
    extensions = ('.mp3', '.flac', '.ogg')
    hashes = {}
    for (root, dirs, files) in os.walk(musicPath, topdown=True):
        for file in files:
            if file.endswith(extensions):
                # music file must be read in binary to calculate hash
                with open(root + "\\" + file, 'rb') as musicFile:
                    hashobj = hashlib.file_digest(musicFile, "sha256")
                    hashes[os.path.relpath(root, musicPath)
                           + '\\' + file] = hashobj.hexdigest()
        # print(f'{root}, {dirs}, {files}')

    # export to json
    with open('metadata.json', 'w') as metadata:
        json.dump(hashes, metadata, indent=4)


def main():
    print("Generating JSON database. This may take a while.")
    generate()
    print("Metadata written.")


if __name__ == '__main__':
    main()
