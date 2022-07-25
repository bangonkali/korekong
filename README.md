# Korekong

## What is the purpose of this program?

This program will recursively search a directory and create MP4 files from MKV files. 

## How the conversion works?

When an MKV file is found, this program picks out the first Audio, the first Video and all properly named Subtitles and creates a MP4 file out of them. The audio track is encoded as [AAC](https://jellyfin.org/docs/general/clients/codec-support.html#audio-compatibilityhttpsenwikipediaorgwikicomparisonofvideocontainerformatsaudiocodingformatssupport-wikipedias-audio-codec-tables), the video track as [H.264](https://jellyfin.org/docs/general/clients/codec-support.html#video-compatibilityhttpsenwikipediaorgwikicomparisonofvideocontainerformats-wikipedias-video-codec-tables) and the subtitles as [mov_text](https://en.wikibooks.org/wiki/FFMPEG_An_Intermediate_Guide/subtitle_options#Set_Subtitle_Codec).

## Why did I make this program?

I wanted a tool to automatically convert all my `MKV` video files in to something that my phone, or for uploading to Youtube. It turns out that [H.264](https://jellyfin.org/docs/general/clients/codec-support.html#video-compatibilityhttpsenwikipediaorgwikicomparisonofvideocontainerformats-wikipedias-video-codec-tables) and [AAC](https://jellyfin.org/docs/general/clients/codec-support.html#audio-compatibilityhttpsenwikipediaorgwikicomparisonofvideocontainerformatsaudiocodingformatssupport-wikipedias-audio-codec-tables) are very well supported formats.

## How to use?

```bash
brew install ffmpeg # You only need to do this once!
korekong -d "path/to/directory"
```

The MP4 files will be created beside the location of MKV files with only the extension name that is different. 

If there is already an MP4 files beside the MKV file that has the same name, then a conversion will not be started for that MKV files to avoid overwriting the existing MP4 File.

# License

MIT License

Copyright (c) 2022 Gil Michael (Bangonkali) Regalado

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.