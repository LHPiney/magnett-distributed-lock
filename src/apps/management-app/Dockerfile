FROM node:20-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine

COPY --from=build /app/dist/management-app/browser /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY inject-env.sh /inject-env.sh

RUN chmod +x /inject-env.sh

EXPOSE 80

CMD ["/inject-env.sh"]

