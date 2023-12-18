```bash
# Add Docker's official GPG key:
sudo apt-get update
sudo apt-get install ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

Для запуска проекта необходимо настроить рабочее окружение для Ubuntu.

Устанавливаем Docker Engine:
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
```

Устанавливаем Docker:
```bash
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```

Устанавливаем и настраиваем nginx:
```bash
apt install nginx
```

Распоковать изображение crypto3:
```bash
docker load -i /root/crypto3.tar
```

В файлах .env, python.env, appsetings.json указать необходимые данные

Собрать проект:
```bash
docker compose up --build
```
