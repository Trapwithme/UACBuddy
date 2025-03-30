# UAC Bypass with ComputerDefaults.exe

This C# proof-of-concept (PoC) demonstrates a User Account Control (UAC) bypass using `computerdefaults.exe` on Windows. It manipulates the registry to silently elevate privileges, launching an admin command prompt to show it worked. Built for educational purposes to explore Windows security.

## Overview
- **What It Does:** Sets a registry key to point to this executable, triggers `computerdefaults.exe` to elevate it without a UAC prompt, then cleans up the registry.
- **Result:** Opens an admin `cmd.exe` if the bypass succeeds.

## Disclaimer
**Educational use only.** Do not use this to harm systems or bypass security without permission—it’s illegal and unethical. The author isn’t liable for misuse. Test responsibly in a controlled environment (e.g., a VM) with consent.

## Notes
- Requires .NET Framework (e.g., 4.7.2).
- Inspired by older UAC bypass techniques, refined here.

## License
Unlicensed—use at your own risk. No warranties.

