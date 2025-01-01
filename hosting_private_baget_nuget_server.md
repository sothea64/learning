# Hosting BaGet NuGet Server on DigitalOcean Droplet and Enabling in Visual Studio

## 1. Introduction
BaGet is a lightweight and simple NuGet server implementation. Hosting BaGet on a DigitalOcean droplet allows you to create a private NuGet server to manage and distribute your NuGet packages.

This guide covers:
1. Hosting BaGet on a DigitalOcean droplet.
2. Configuring Visual Studio to use your private NuGet server.

---

## 2. Hosting BaGet on DigitalOcean

### Prerequisites
- A DigitalOcean account.
- A droplet running Ubuntu (preferably 20.04 or newer).
- A domain name or IP address for accessing your NuGet server.
- SSH access to your droplet.

### Step 1: Update the Droplet
1. Connect to your droplet via SSH:
   ```bash
   ssh root@<your-droplet-ip>
   ```
2. Update the system packages:
   ```bash
   sudo apt update && sudo apt upgrade -y
   ```

### Step 2: Install Docker and Docker Compose
1. Install Docker:
   ```bash
   sudo apt install -y docker.io
   ```
2. Install Docker Compose:
   ```bash
   sudo apt install -y docker-compose
   ```
3. Enable and start Docker:
   ```bash
   sudo systemctl enable docker
   sudo systemctl start docker
   ```

### Step 3: Create a Directory for BaGet
1. Create a directory for BaGet files:
   ```bash
   mkdir -p ~/baget && cd ~/baget
   ```

### Step 4: Create a `docker-compose.yml` File
1. Create a `docker-compose.yml` file:
   ```bash
   nano docker-compose.yml
   ```
2. Add the following content to the file:
   ```yaml
   version: '3.7'

   services:
     baget:
       image: loicsharma/baget:latest
       container_name: baget
       ports:
         - "5000:5000"
       environment:
         - Baget__Database__Type=Sqlite
         - Baget__Search__Type=Database
       volumes:
         - ./data:/var/baget
       restart: always
   ```
3. Save and exit (`Ctrl+O`, `Enter`, `Ctrl+X`).

### Step 5: Start BaGet
1. Start the BaGet container:
   ```bash
   docker-compose up -d
   ```
2. Verify that BaGet is running:
   ```bash
   docker ps
   ```
   You should see a container named `baget` running.

3. Access BaGet in your browser using the droplet's IP address and port `5000`:
   ```
   http://<your-droplet-ip>:5000
   ```

### Step 6: Configure a Domain (Optional)
If you want to access BaGet via a custom domain (e.g., `nuget.example.com`):

1. Point your domain to the droplet's IP address using an **A Record**.
2. Set up a reverse proxy with Nginx:
   ```bash
   sudo apt install -y nginx
   ```
   Create an Nginx configuration:
   ```bash
   sudo nano /etc/nginx/sites-available/baget
   ```
   Add the following:
   ```nginx
   server {
       listen 80;
       server_name nuget.example.com;

       location / {
           proxy_pass http://localhost:5000;
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```
   Enable the configuration:
   ```bash
   sudo ln -s /etc/nginx/sites-available/baget /etc/nginx/sites-enabled/
   sudo systemctl restart nginx
   ```
3. Access BaGet using your domain:
   ```
   http://nuget.example.com
   ```

---

## 3. Enable NuGet Server in Visual Studio

### Step 1: Open NuGet Package Manager Settings
1. In Visual Studio, go to **Tools** > **NuGet Package Manager** > **Package Manager Settings**.

### Step 2: Add Your BaGet Server
1. Select **Package Sources** from the left menu.
2. Click the **+** button to add a new source.
3. Fill in the details:
   - **Name**: BaGet Server
   - **Source**: Your BaGet server URL (e.g., `http://<your-droplet-ip>:5000/v3/index.json` or `http://nuget.example.com/v3/index.json`)
4. Click **Update** and then **OK**.

### Step 3: Test the Configuration
1. Open the **Manage NuGet Packages** window by right-clicking on your project and selecting **Manage NuGet Packages**.
2. Select your BaGet source from the drop-down in the top-right corner.
3. You should see the packages hosted on your BaGet server.

---

## 4. Upload Packages to BaGet

### Step 1: Create a NuGet Package
1. Use the `dotnet pack` command to create a `.nupkg` file:
   ```bash
   dotnet pack -o ./nupkgs
   ```

### Step 2: Push the Package to BaGet
1. Use the `dotnet nuget push` command to upload the package:
   ```bash
   dotnet nuget push ./nupkgs/<your-package>.nupkg --source "http://<your-droplet-ip>:5000/v3/index.json"
   ```
2. If your BaGet server requires authentication, include `--api-key` (though BaGet typically doesn't require it by default).
