#!/usr/bin/env bash
# Cập nhật phiên bản mới lên VPS (chạy từ máy dev, Git Bash).
# Yêu cầu: SSH key ~/.ssh/webbanhoa_vps đã được thêm vào /root/.ssh/authorized_keys trên VPS.
set -e
VPS=root@14.225.220.49
KEY=~/.ssh/webbanhoa_vps
SITE=/www/wwwroot/hoangtrung.vnc360.com
cd "$(dirname "$0")/.."

dotnet publish WebBanHoa/WebBanHoa.csproj -c Release -r linux-x64 --self-contained true -o /tmp/webbanhoa-publish
tar czf /tmp/webbanhoa-linux.tar.gz -C /tmp/webbanhoa-publish .
scp -i $KEY /tmp/webbanhoa-linux.tar.gz $VPS:/tmp/
ssh -i $KEY $VPS "systemctl stop webbanhoa && rm -rf $SITE/app/* && tar xzf /tmp/webbanhoa-linux.tar.gz -C $SITE/app \
  && rm /tmp/webbanhoa-linux.tar.gz && chmod +x $SITE/app/WebBanHoa && chown -R www:www $SITE && systemctl start webbanhoa \
  && sleep 5 && systemctl is-active webbanhoa"
