import os
import random
import shutil
from pydub.generators import WhiteNoise


def generateMP3(output_path, duration_ms):
    # Generate white noise of the given duration
    noise = WhiteNoise().to_audio_segment(duration=duration_ms)
    noise.export(output_path, format="mp3")
    # It is NOT RECOMMENDED to listen to any of the output,
    # as the white noise is quite harsh.


def generateFolder(base_path, num_artists=2,
                   albums_per_artist=2,
                   files_per_album=3):
    for artist_num in range(1, num_artists + 1):
        artist_path = os.path.join(base_path, f"Artist_{artist_num}")
        os.makedirs(artist_path, exist_ok=True)

        for album_num in range(1, albums_per_artist + 1):
            album_path = os.path.join(artist_path, f"Album_{album_num}")
            os.makedirs(album_path, exist_ok=True)

            for file_num in range(1, files_per_album + 1):
                file_name = f"Track_{file_num}.mp3"
                file_path = os.path.join(album_path, file_name)
                duration_ms = random.randint(1000, 5000)
                generateMP3(file_path, duration_ms)
                print(f"Generated {file_name}...")


def randomCopy(source_folder,
               destination_folder,
               copy_chance=0.25):
    for root, _, files in os.walk(source_folder):
        for file in files:
            extensions = (".mp3", ".flac", ".ogg")
            if file.endswith(extensions) and random.random() < copy_chance:
                src_file = os.path.join(root, file)
                rel_path = os.path.relpath(src_file, source_folder)
                copy_file = os.path.join(destination_folder, rel_path)

                # Ensure the destination directory exists
                destination_dir = os.path.dirname(copy_file)
                os.makedirs(destination_dir, exist_ok=True)

                # Copy the file
                shutil.copy2(src_file, copy_file)
                print(f"Copied: {src_file} -> {copy_file}")


def reset(folder1, folder2):
    li = (folder1, folder2)
    for x in li:
        if os.path.exists(x):
            shutil.rmtree(x)
            print(f"{x} and its contents have been deleted.")
        elif not os.path.exists(x):
            print(f"{x} does not exist.")
        else:
            print(f"{x} was not removed.")


def main():
    dir1 = "TestFolder1"
    dir2 = "TestFolder2"
    reset(dir1, dir2)
    generateFolder(dir1, 10, 3, 6)
    print(f"Random MP3 files have been generated in {dir1}")
    randomCopy(dir1, dir2)
    print(f"Files from {dir1} have been copied to {dir1}.")


if __name__ == "__main__":
    main()
