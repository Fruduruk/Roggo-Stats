FROM egui-wasm-builder:rust-1.95-trunk AS build

WORKDIR /app
COPY . .
RUN trunk build --release

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80