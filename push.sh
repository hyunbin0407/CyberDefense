#!/bin/bash

# CyberDefense GitHub 업로드 스크립트

cd "$(dirname "$0")"

echo "변경된 파일 확인 중..."
git status

echo ""
read -p "커밋 메시지를 입력하세요 (엔터 = 'Update CyberDefense'): " msg
msg="${msg:-Update CyberDefense}"

git add .
git commit -m "$msg"
git push origin main

echo ""
echo "GitHub에 업로드 완료!"
echo "확인: https://github.com/hyunbin0407/CyberDefense"
