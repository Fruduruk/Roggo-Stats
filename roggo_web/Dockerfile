FROM registry.local.frudd.dev/egui-wasm-builder:latest AS build

WORKDIR /app

COPY roggo_web ./roggo_web
COPY roggo_contract ./roggo_contract

WORKDIR /app/roggo_web
RUN CARGO_BUILD_JOBS=2 trunk build --release

FROM nginx:alpine
COPY --from=build /app/roggo_web/dist /usr/share/nginx/html
EXPOSE 80