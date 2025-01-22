import generatemp3s


# Utilize the reset function from the random mp3 generator to clean the repo
# for uploading to github. Folder names are hardcoded because they
# will *never* change.
def main():
    print("Running upload prep script...")
    generatemp3s.reset("TestFolder1", "TestFolder2")
    print("Complete.")


if __name__ == "__main__":
    main()
