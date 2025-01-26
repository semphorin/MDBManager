from hashlib import file_digest
import os
import yaml
import json
from time import time


def compute_binary_hash(file, algo='md5'):
    hashobj = file_digest(file, algo)
    return hashobj.hexdigest()


def generate():
    with open('musicpath.yaml') as yamlfile:
        yamldict = yaml.safe_load(yamlfile)

    musicPath = yamldict['musicPath']

    # go to musicPath, walk through every dir+subdir and catalog relative path
    # plus file hash. store in dictionary
    extensions = ('.mp3', '.flac', '.ogg')
    hashes = {}
    start = time()
    for (root, dirs, files) in os.walk(musicPath, topdown=True):
        for file in files:
            if file.endswith(extensions):
                # music file must be read in binary to calculate hash
                with open(root + '/' + file, 'rb') as musicFile:
                    relativePath = os.path.relpath(root, musicPath).replace("\\", "/") + '/' + file
                    hashes[relativePath] = compute_binary_hash(musicFile)

    print(f"Time to generate MD5: {round(time() - start, 3)} seconds")

    # export to json
    with open('metadata.json', 'w') as metadata:
        json.dump(hashes, metadata, indent=4)

    print(f"Time to write metadata: {round(time() - start, 3)} seconds")


def main():
    print("Generating JSON database. This may take a while.")
    generate()
    print("Metadata written.")


if __name__ == '__main__':
    main()
