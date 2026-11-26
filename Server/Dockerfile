FROM node:22-alpine
WORKDIR /app
COPY package.json ./
COPY relay-server.js ./
ENV NODE_ENV=production
ENV PORT=8787
EXPOSE 8787
USER node
CMD ["node","relay-server.js"]
