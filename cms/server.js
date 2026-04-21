const express = require("express");
const cors    = require("cors");
const multer  = require("multer");
const path    = require("path");
const fs      = require("fs");

const app  = express();
const PORT = process.env.PORT || 3000;

// ── Paths ──────────────────────────────────────────────────────────────────────

const ROOT_DIR   = __dirname;
const THEMES_DIR = path.join(ROOT_DIR, "themes");
const ICONS_DIR  = path.join(ROOT_DIR, "icons");
const IMAGES_DIR = path.join(ROOT_DIR, "images");

// ── Middleware ─────────────────────────────────────────────────────────────────

app.use(cors());
app.use(express.json());

// Static file serving
// GET /themes/desktop-theme.json  →  themes/desktop-theme.json
// GET /icons/home.png             →  icons/home.png
// GET /images/start-bg.png        →  images/start-bg.png
app.use("/themes", express.static(THEMES_DIR));
app.use("/icons",  express.static(ICONS_DIR));
app.use("/images", express.static(IMAGES_DIR));

// ── Theme save (CMS) ───────────────────────────────────────────────────────────

// POST /themes/desktop-theme.json  – save edited theme back to disk
// POST /themes/mobile-theme.json   – save edited mobile theme back to disk
app.post("/themes/:filename", (req, res) => {
  const filename = req.params.filename;

  if (!filename.endsWith(".json")) {
    return res.status(400).json({ error: "Only .json files can be saved" });
  }

  const data = req.body;
  if (!data || typeof data !== "object") {
    return res.status(400).json({ error: "Invalid JSON body" });
  }

  const filePath = path.join(THEMES_DIR, filename);
  fs.writeFileSync(filePath, JSON.stringify(data, null, 2), "utf8");
  console.log(`[CMS] Saved: themes/${filename}`);
  res.json({ ok: true });
});

// ── File upload ────────────────────────────────────────────────────────────────

const UPLOAD_FOLDERS = {
  icons:  ICONS_DIR,
  images: IMAGES_DIR,
};

const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    const dest = UPLOAD_FOLDERS[req.params.folder];
    if (!dest) return cb(new Error(`Unknown folder: ${req.params.folder}`));
    cb(null, dest);
  },
  filename: (req, file, cb) => {
    cb(null, file.originalname);
  },
});

const upload = multer({
  storage,
  fileFilter: (req, file, cb) => {
    const allowed = /\.(png|jpg|jpeg|gif|webp|svg)$/i;
    if (!allowed.test(file.originalname)) {
      return cb(new Error("Only image files are allowed"));
    }
    cb(null, true);
  },
});

// POST /upload/icons   – upload a file to icons/
// POST /upload/images  – upload a file to images/
app.post("/upload/:folder", upload.single("file"), (req, res) => {
  if (!req.file) return res.status(400).json({ error: "No file received" });
  const url = `/${req.params.folder}/${req.file.originalname}`;
  console.log(`[CMS] Uploaded: ${url}`);
  res.json({ ok: true, url });
});

// ── Error handler ──────────────────────────────────────────────────────────────

app.use((err, req, res, next) => {
  console.error("[Server error]", err.message);
  res.status(500).json({ error: err.message });
});

// ── Start ──────────────────────────────────────────────────────────────────────

app.listen(PORT, () => {
  console.log(`Configurator CMS running at http://localhost:${PORT}`);
  console.log(`  Themes: ${THEMES_DIR}`);
  console.log(`  Icons:  ${ICONS_DIR}`);
  console.log(`  Images: ${IMAGES_DIR}`);
});
