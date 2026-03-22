FROM python:3.10.20-slim
WORKDIR /app

ARG APP_VERSION=dev
ENV APP_VERSION=$APP_VERSION
RUN echo "$APP_VERSION"

COPY . ./

RUN python -m pip install --upgrade pip
RUN pip install --no-cache-dir -r requirements.txt

CMD ["python", "-u", "./main.py"]