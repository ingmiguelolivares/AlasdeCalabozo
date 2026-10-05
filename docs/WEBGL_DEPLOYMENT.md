# WebGL deployment checklist

## Build base

1. Use `Brotli` compression for release builds.
2. Keep `Decompression Fallback` disabled when the server supports `Content-Encoding: br`.
3. Keep `Data Caching` enabled.
4. Use file hashes for cacheable build files.
5. Use a lightweight first scene and load heavy content after the menu.

## Addressables

1. Open Unity.
2. Run `Tools > WebGL Optimization > Prepare Project For WebGL`.
3. Open `Window > Asset Management > Addressables > Groups`.
4. Confirm `Remote_Cards` contains the card sprites.
5. Set production `Remote.LoadPath` to the CDN URL.
6. Build Addressables with `Build > New Build > Default Build Script`.

## Server

Use the config samples in `webgl-server/` for Nginx, Apache, IIS, Netlify, or Cloudflare-style `_headers`.

Critical headers:

- `.br` files: `Content-Encoding: br`
- `.wasm.br`: `Content-Type: application/wasm`
- `.js.br`: `Content-Type: application/javascript`
- hashed build files: `Cache-Control: public, max-age=31536000, immutable`
- `index.html`: `Cache-Control: no-cache`

If the hosting cannot set these headers, use gzip or enable Unity decompression fallback as a compatibility option.
