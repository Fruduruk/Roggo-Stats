# deploy tutorial

#### 1. Install Docker on your system

#### 2. Run ``docker build -t ballchasingwrapper:{version} .`` to build the image.

#### 2.5 Run ``docker save ballchasingwrapper:{version} -o ballchasingwrapper.tar`` if you deploy on a different machine and need a file.

#### 3. Run ``docker-compose up`` to deploy the whole thing.