
# Docker TLS Configuration Guide

This guide outlines the steps to enable secure TLS remote access for Docker between a server and a client.

---

## **1. Prerequisites**
- A server with Docker installed.
- A client machine to access Docker remotely.
- OpenSSL installed on the server.
- Ports configured on the server's firewall (default: `2376` for secure Docker connections).
- Follow this instruction to allow remote access docker daemon https://gist.github.com/styblope/dc55e0ad2a9848f2cc3307d4819d819f

---

## **2. Generate Certificates on the Server**

### **Step 2.1: Create a Directory for Certificates**
```bash
mkdir ~/docker_certs && cd ~/docker_certs
```

### **Step 2.2: Generate CA Certificate**
1. Create a private key for the CA:
   ```bash
   openssl genrsa -aes256 -out ca-key.pem 4096
   ```

2. Create the CA certificate:
   ```bash
   openssl req -new -x509 -days 365 -key ca-key.pem -sha256 -out ca.pem
   ```

---

### **Step 2.3: Generate Server Certificate**
1. Create a private key for the server:
   ```bash
   openssl genrsa -out server-key.pem 4096
   ```

2. Create a certificate signing request (CSR) for the server:
   ```bash
   openssl req -subj "/CN=<YOUR_SERVER_IP>" -new -key server-key.pem -out server.csr
   ```

3. Create a configuration file for the server certificate:
   ```bash
   echo "subjectAltName = IP:<YOUR_SERVER_IP>" > extfile.cnf
   echo "extendedKeyUsage = serverAuth" >> extfile.cnf
   ```

4. Sign the server certificate using the CA:
   ```bash
   openssl x509 -req -days 365 -sha256 -in server.csr -CA ca.pem -CAkey ca-key.pem -CAcreateserial -out server-cert.pem -extfile extfile.cnf
   ```

---

### **Step 2.4: Generate Client Certificate**
1. Create a private key for the client:
   ```bash
   openssl genrsa -out client-key.pem 4096
   ```

2. Create a CSR for the client:
   ```bash
   openssl req -subj '/CN=client' -new -key client-key.pem -out client.csr
   ```

3. Create a configuration file for the client certificate:
   ```bash
   echo "extendedKeyUsage = clientAuth" > extfile-client.cnf
   ```

4. Sign the client certificate using the CA:
   ```bash
   openssl x509 -req -days 365 -sha256 -in client.csr -CA ca.pem -CAkey ca-key.pem -CAcreateserial -out client-cert.pem -extfile extfile-client.cnf
   ```

---

## **3. Configure Docker on the Server**

### **Step 3.1: Move Server Certificates**
Move the server's certificates to the Docker configuration directory:
```bash
sudo mkdir -p /etc/docker/certs
sudo mv ca.pem server-cert.pem server-key.pem /etc/docker/certs
```

### **Step 3.2: Edit Docker Daemon Configuration**
Edit the Docker daemon configuration file:
```bash
sudo nano /etc/docker/daemon.json
```

Add the following:
```json
{
  "hosts": ["tcp://0.0.0.0:2376", "unix:///var/run/docker.sock"],
  "tls": true,
  "tlscacert": "/etc/docker/certs/ca.pem",
  "tlscert": "/etc/docker/certs/server-cert.pem",
  "tlskey": "/etc/docker/certs/server-key.pem",
  "tlsverify": true
}
```

### **Step 3.3: Restart Docker**
```bash
sudo systemctl restart docker
```

---

## **4. Configure the Client**

### **Step 4.1: Copy Certificates to the Client**
Copy the following files from the server (`~/docker_certs`) to the client machine:
- `ca.pem`
- `client-cert.pem` (rename to `cert.pem`)
- `client-key.pem` (rename to `key.pem`)

For example:
```bash
scp <user>@<YOUR_SERVER_IP>:~/docker_certs/ca.pem ~/
scp <user>@<YOUR_SERVER_IP>:~/docker_certs/client-cert.pem ~/cert.pem
scp <user>@<YOUR_SERVER_IP>:~/docker_certs/client-key.pem ~/key.pem
```

Move them to the `~/.docker` directory on the client:
```bash
mkdir -p ~/.docker
mv ~/ca.pem ~/cert.pem ~/key.pem ~/.docker
```

---

### **Step 4.2: Configure Environment Variables**
Set environment variables to configure the Docker client:
```bash
export DOCKER_TLS_VERIFY="1"
export DOCKER_CERT_PATH="~/.docker"
export DOCKER_HOST="tcp://<YOUR_SERVER_IP>:2376"
```

To make these settings permanent, add them to your `~/.bashrc` or `~/.zshrc`:
```bash
echo 'export DOCKER_TLS_VERIFY="1"' >> ~/.bashrc
echo 'export DOCKER_CERT_PATH="~/.docker"' >> ~/.bashrc
echo 'export DOCKER_HOST="tcp://<YOUR_SERVER_IP>:2376"' >> ~/.bashrc
source ~/.bashrc
```

---

## **5. Test the Connection**
Run the following command from the client to verify the connection:
```bash
docker info
```

If everything is configured correctly, you'll see information about the Docker daemon running on the server.

---

## **Important Notes**
1. **Firewall Configuration**: Ensure port `2376` is open on the server's firewall.
   ```bash
   sudo ufw allow 2376/tcp
   ```

2. **File Naming**:
   - `ca.pem` remains as is.
   - Rename `client-cert.pem` to `cert.pem`.
   - Rename `client-key.pem` to `key.pem`.

3. **Debugging**:
   - If you encounter issues, check the Docker logs on the server:
     ```bash
     sudo journalctl -u docker
     ```
   - Use OpenSSL to verify the certificate chain:
     ```bash
     openssl s_client -connect <YOUR_SERVER_IP>:2376 -CAfile ~/.docker/ca.pem
     ```

By following this guide, you can securely configure Docker with TLS for remote access.
