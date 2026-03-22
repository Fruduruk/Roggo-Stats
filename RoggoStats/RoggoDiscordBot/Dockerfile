FROM python:3.14.3-slim-trixie
WORKDIR /app

ARG APP_VERSION=dev
ENV APP_VERSION=$APP_VERSION
RUN echo "$APP_VERSION"

COPY . ./

RUN python -m pip install --upgrade pip
RUN pip install --no-cache-dir -r requirements.txt

CMD ["python", "-u", "./main.py"]