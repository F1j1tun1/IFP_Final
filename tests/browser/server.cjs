const http = require("node:http");
const fs = require("node:fs");
const path = require("node:path");
const mime = {
    ".html": "text/html",
    ".css": "text/css",
    ".js": "text/javascript",
    ".json": "application/json",
    ".wasm": "application/wasm",
    ".png": "image/png",
    ".svg": "image/svg+xml",
    ".dat": "application/octet-stream",
};
function serve(root, port = 5080) {
    root = path.resolve(root);
    const server = http.createServer((req, res) => {
        const pathname = decodeURIComponent(
            new URL(req.url, "http://localhost").pathname,
        );
        if (!pathname.startsWith("/IFP_Final/")) {
            res.writeHead(404);
            res.end();
            return;
        }
        const file = path.resolve(
            root,
            pathname.slice("/IFP_Final/".length) || "index.html",
        );
        if (!file.startsWith(root + path.sep)) {
            res.writeHead(403);
            res.end();
            return;
        }
        fs.readFile(file, (error, data) => {
            res.writeHead(error ? 404 : 200, {
                "Content-Type":
                    mime[path.extname(file)] || "application/octet-stream",
                "Cache-Control": "no-store",
            });
            res.end(error ? "Not found" : data);
        });
    });
    return new Promise((resolve) =>
        server.listen(port, "127.0.0.1", () => resolve(server)),
    );
}
module.exports = { serve };
if (require.main === module)
    serve(process.argv[2] || "artifacts/pages/wwwroot").then(() =>
        console.log("http://127.0.0.1:5080/IFP_Final/"),
    );
