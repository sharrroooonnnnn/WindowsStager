import random, sys

def xor_encrypt(data, key):
    result = bytearray()
    for i in range(len(data)):
        result.append(data[i] ^ key[i % len(key)])
    return result

if len(sys.argv) < 2:
    print("Usage: python encrypt.py <shellcode_file> [key]")
    sys.exit(1)

# Read the shellcode file
with open(sys.argv[1], "rb") as f:
    shellcode = f.read()

if len(sys.argv) > 2:
    # Use the key provided by the user
    key = bytearray.fromhex(sys.argv[2])
else:
    # Generate a random key
    key = bytearray(random.getrandbits(8) for i in range(4))

# Encrypt the shellcode with the key
encrypted_shellcode = xor_encrypt(shellcode, key)

# Print the key as hex
print("Key: ", key.hex())

# Write the encrypted shellcode to a file
with open("encrypted_shellcode.bin", "wb") as f:
    f.write(encrypted_shellcode)
