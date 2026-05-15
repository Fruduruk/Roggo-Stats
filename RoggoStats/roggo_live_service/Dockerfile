FROM egui-wasm-builder:1.95-trunk AS build

WORKDIR /app
COPY . .
RUN CARGO_BUILD_JOBS=2 trunk build --release

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80