# Java---CMU-Sphinx---Text-to-Language-Model
Includes dictionaries for English, Dutch and German

Creates ARPA Language Model using CMU Sphinx libraries and optional dictionary. 
The dictionary will only contain words, which are present in your language model, so you'll be saving some memory!

Input a normal text file (utf-8) special characters will be removed

Usage:
  1. Open a console window at your jars location 
  2. java -jar TextToLanguageModel.jar -input \<PathToTextFile> [-dict] ( \<PathToDictionaryFile> | en | nl | de )
  3. If errors occur, try starting your console in admin
