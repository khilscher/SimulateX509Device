# SimulateX509Device
Code sample to simulate a device connected to IoT Hub using an x.509 certificate.

# Setup

- In IoT Hub:
  -Under **Certificates**, upload your signing certificate (e.g. Intermediate cert) and verify it.
  -Under **IoT Device**, create a new IoT device. Select ```X.509 CA Signed`` and make note of the ```Device ID``` you create.
- Generate a certificate for your device as follows. In this example, the ```Device ID``` is **device123**.
```
// Generate key pair for device; output is a pem file (pkcs8 format)
openssl genrsa -out device123.key 2048

// Generate CSR. Fill in the questions and ensure CN = device123
openssl req -new -key device123.key -out device123.csr

// Generate device cert signed using intermediate CA cert and key
openssl x509 -req -days 1730 -in device123.csr -CA ia.cer -CAkey ia.key -set_serial 01 -out device123.cer

// The IoT Device SDK needs both the Signed Certificate as well as the private key information. 
// It expects to load a single PFX-formatted Bundle containing all necessarily information.
// We can combine the Key and Certificate to a PFX archive as follows:
openssl pkcs12 -export -out device123.pfx -inkey device123.key -in device123.cer 

```
- Open ```Program.cs``` and update the following:
  -```deviceId```
  -```pfxPath```
  -```iotHubFqdn```
- Build and run the sample.


