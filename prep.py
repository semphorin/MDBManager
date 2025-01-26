import generateMP3s
from os import remove


# Utilize the reset function from the random mp3 generator to clean the repo
# for uploading to github. Folder names are hardcoded because they
# will *never* change.
def removeExcess():
    # this may expand as more user-specific files get added to the program
    with open('Config/musicpath.yaml', 'w') as musicpathfile:
        musicpathfile.write("musicPath: ''")
    print("musicpath.yaml has been reset.")

    try:
        remove('metadata.json')
        print("metadata.json has been removed.")
    except Exception:
        print('Error removing file: metadata.json does not exist.')

    try:
        remove('bin/Debug/net8.0/metadata.json')
        print("bin///metadata.json has been removed.")
    except Exception:
        print('Error removing file: bin/metadata.json does not exist.')

    try:
        remove('diff.zip')
        print("diff.zip has been removed.")
    except Exception:
        print('Error removing file: diff.zip does not exist.')


def main():
    print("Running upload prep script...")
    removeExcess()
    generateMP3s.reset("TestFolder1", "TestFolder2")
    print("Complete.")


if __name__ == "__main__":
    main()
