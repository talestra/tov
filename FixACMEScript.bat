@echo off
"%~dp0\Tools\FixACMEScript" FixACMEScript.txt "PatchData\Text\tov.json"
"%~dp0\Tools\StripACMEScript" "PatchData\Text\tov.json" "PatchData\Text\tov_cleaned.json"