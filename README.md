# Product Configurator — Developer Reference

## Overview

Unity WebGL product configurator built entirely with Unity UI Toolkit (UITK). Supports desktop and mobile layouts from a single codebase, switching at runtime based on screen width. All UI text, colors, fonts, and icons are driven at runtime from JSON files served by an external CMS server — no Unity rebuild required to change branding or content. A loading screen with progress bar shows during boot, and an error panel with retry appears if the server is unreachable.

---

## Architecture Overview

```
CMS Server (Node.js)
  themes/desktop-theme.json
  themes/mobile-theme.json
  icons/
  images/
        ↓  HTTP (UnityWebRequest)
ThemeLoader              — fetches JSON + downloads all image assets
        ↓
RuntimeThemeData         — resolved Unity types (Color, Font, Texture2D)
        ↓
UIThemeApplicator        — writes to cached element refs
        ↓
HomeScreenUI             — caches all element refs at startup
        ↓
HomeScreen               — MonoBehaviour orchestrator
```

---

## Scene Setup

The following components must be present and configured in the scene.

### Required GameObjects and Components

**HomeScreen** (MonoBehaviour)
Assign in Inspector:
- `uiDocument` — the UIDocument component holding the active instance UXML
- `cameraController`, `decalController`, `rotationTarget`, `spawnPoint`
- `swoopPosition`, `startPosition`, `productViewPosition`, `videoPosition` — camera anchor Transforms
- `productButtonTemplate`, `specRowTextTemplate`, `specRowBarTemplate`, `specRowToggleTemplate`, `specRowChipsTemplate`, `inspectRowTemplate` — VisualTreeAsset references
- `video_FOV`, `normal_FOV` — camera field of view values
- `resourcesPath` — path inside Resources folder where Product assets are stored (default: `GamyraDrive`)
- `defaultProductId` — product ID to select on startup
- `currentProductId` — product ID selected currently (assign same as default)

**HomeScreenThemeBootstrap** (MonoBehaviour)
Orchestrates theme loading. Assign in Inspector:
- `themeLoader` — reference to the ThemeLoader component
- `themeFontLibrary` — reference to the ThemeFontLibrary component
- `desktopThemeFile` — server-relative path to desktop theme (default: `themes/desktop-theme.json`)
- `mobileThemeFile` — server-relative path to mobile theme (default: `themes/mobile-theme.json`)

**ThemeLoader** (MonoBehaviour)
Fetches theme JSON and downloads image assets. Assign in Inspector:
- `cmsBaseUrl` — base URL of the CMS server (default: `http://localhost:3000`)

**ThemeFontLibrary** (MonoBehaviour)
Holds font assets keyed by string. Assign in Inspector:
- Add entries to the `fonts` list — each entry has a `key` (matches the font key in theme JSON) and a `font` (Unity Font asset)

**DeviceDetection** (MonoBehaviour)
Controls which UIDocument GameObject is active. Assign `desktopUI` and `mobileUI` GameObject references. Set `mode` to `Auto` for production — detects mobile via `Application.isMobilePlatform` or screen width ≤ 900px on WebGL.

**UIBlockerRaycast** (MonoBehaviour)
Prevents camera rotation when the pointer is over UI. Assign the active `UIDocument`. Must be in the scene for camera input to work correctly.

**CameraController** (MonoBehaviour)
Assign `homeScreen` reference in Inspector. Assign `Cornea` (CorneaCameraDirector). Configure `rotationSpeed`, `zoomSpeed`, `minDistance`, `maxDistance`.

**WebGLDownloadManager** (MonoBehaviour)
Singleton. Set `pdfBaseUrl` to the server path where PDF brochures are hosted.

**ScreenshotHandler** (MonoBehaviour)
Singleton. No configuration required. Requires the WebGL JS plugin to define `VShowroom_DownloadScreenshot`.

**DecalController** (MonoBehaviour on the decal projector GameObject)
Configure `fadeInDuration`, `fadeOutDuration`, `scaleUpDuration`, `scaleDownDuration`, `maxSize` in Inspector.

---

## CMS Server

A lightweight Node.js/Express server that serves theme JSON and static assets. Unity fetches all theme data from this server at runtime.

### Setup

```bash
cd server
npm install
npm run dev     # development with auto-restart
npm start       # production
```

Server runs at `http://localhost:3000` by default.

### Folder Structure

```
server/
  server.js
  package.json
  themes/
    desktop-theme.json
    mobile-theme.json
  icons/
    home.png
    product.png
    ...
  images/
    start-button.png
    home-tab-bg.png
    ...
```

### Endpoints

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/themes/desktop-theme.json` | Unity fetches desktop theme on startup |
| `GET` | `/themes/mobile-theme.json` | Unity fetches mobile theme on startup |
| `GET` | `/icons/:filename` | Serves icon images |
| `GET` | `/images/:filename` | Serves background images |
| `POST` | `/themes/:filename` | CMS saves edited theme JSON to disk |
| `POST` | `/upload/icons` | Uploads an image to `icons/` |
| `POST` | `/upload/images` | Uploads an image to `images/` |

### Changing the server URL in Unity

Update the `Cms Base Url` field on the `ThemeLoader` component in the Inspector. For production, point this to your live server URL.

---

## Theme JSON Structure

Each theme file (`desktop-theme.json`, `mobile-theme.json`) follows this structure:

```json
{
  "fonts": {
    "bodyFontKey":  "archivo-regular",
    "boldFontKey":  "archivo-bold",
    "lightFontKey": "archivo-light",
    "bodyFontSize": 18,
    "boldFontSize": 30,
    "tabFontSize":  15,
    "titleFontSize": 18,
    "buttonFontSize": 16,
    "smallFontSize": 10
  },
  "text": {
    "brandLogoText": "GAMYRA",
    "startButtonText": "CLICK HERE TO START",
    ...
  },
  "colors": {
    "accentPrimary": "#FDC048FF",
    "primaryText":   "#FFFFFFFF",
    ...
  },
  "images": {
    "iconHome":    "/icons/home.png",
    "startButtonBackground": "/images/start-button.png",
    ...
  }
}
```

Colors use `#RRGGBBAA` hex format. Image paths are server-relative (prefixed with the base URL at runtime). Font keys must match entries in the `ThemeFontLibrary` Inspector list. Leave any image field as `""` to skip loading that asset.

---

## Adding a Product

1. Right-click in the Project panel → Create → Product Configurator → Product
2. Fill in `productName` and `productId` — the ID must be unique across all products
3. Assign `productPrefab`
4. Toggle `showSpecs` and `showInspect` to control which panels appear for this product
5. Add `SpecRow` entries under Specs — choose the type (Text, Bar, InvertedBar, Toggle, Chips) and fill the relevant fields
6. Add `InspectPoint` entries — each needs a label and a `cameraAnchor` Transform in the scene (created during development)
7. Optionally fill `brochurePdfFile` (filename relative to `pdfBaseUrl`) and `brochureDownloadName`
8. Place the Product asset inside the Resources folder at the path matching `resourcesPath` on HomeScreen

Products are loaded automatically at startup via `Resources.LoadAll` and sorted by `productId`.

---

## UI Architecture

Four layers with strict one-way dependencies:

```
ThemeLoader / HomeScreenThemeBootstrap  (fetches + resolves all theme data)
        ↓
RuntimeThemeData    (resolved Color, Font, Texture2D — ready for direct use)
        ↓
UIThemeApplicator   (reads RuntimeThemeData, writes to cached element refs)
        ↓
HomeScreenUI        (caches all element refs + StyleTargets at startup)
        ↓
HomeScreen          (MonoBehaviour — orchestrates everything)
```

**`UINames.cs`** sits outside this chain as a shared registry. Every UXML element name and USS class name used in code is defined here as a constant. No script contains a hardcoded UI string.

### Startup sequence

1. `HomeScreen.OnEnable` — creates `HomeScreenThemeController` and starts boot
2. `HomeScreenThemeController.Bootstrap()` — shows loading screen with progress bar, fetches theme JSON and all images from the CMS server
3. Boot always completes (success or failure) and calls `OnThemeReady` — this ensures the UITK input system is fully initialized on every path
4. `OnThemeReady` runs `ApplyThemeAndBuildControllers()` — applies theme, builds all controllers, binds all buttons including the error panel retry button
5. If theme loaded successfully → welcome screen is shown
6. If theme failed → error panel is shown with a Retry button. Clicking Retry restarts from step 2.

### Boot failure handling

If the CMS server is unreachable or the theme JSON is invalid, the app shows an error panel with a Retry button. The full UI initialization always runs regardless of theme load result — this is intentional, as UITK's input system requires a complete initialization pass before buttons become clickable. The error panel button is bound through the same `BindButtons` path as all other UI buttons.

---

## Customising the Theme

Edit `themes/desktop-theme.json` or `themes/mobile-theme.json` in the CMS server folder. No Unity rebuild required — restart the server and hit Play (or reload the WebGL build).

### FontData

| Field | Controls | Default |
|---|---|---|
| `bodyFontKey` | Key for body font in ThemeFontLibrary | — |
| `boldFontKey` | Key for bold/logo font | — |
| `lightFontKey` | Key for light font | — |
| `bodyFontSize` | Body text size | 18px |
| `boldFontSize` | Logo text size | 30px |
| `tabFontSize` | Tab button text size | 15px |
| `titleFontSize` | Panel section title size | 18px |
| `buttonFontSize` | Action button text size | 16px |
| `smallFontSize` | Reset view label size | 10px |

Font assets must be assigned in the `ThemeFontLibrary` Inspector list. The JSON keys link to those assets by name.

### TextData

Every visible string in the UI — welcome screen text, tab labels, button labels, section titles, navigation labels, info overlay body text.

### ColorData

| Field | Controls |
|---|---|
| `accentPrimary` | Active tab color, active nav icon background |
| `brandLogoColor` | Logo labels, welcome title |
| `primaryText` | All body labels |
| `secondaryText` | Inactive tabs, separators |
| `actionButtonText` | Text on NEXT / DONE buttons |
| `welcomeBg` | Welcome screen background |
| `topNavBg` | Desktop and mobile top nav bar background |
| `panelCardBg` | Desktop and mobile panel card/section background |
| `overlayBackdropBg` | Info overlay backdrop |
| `infoCardBg` | Info overlay card background |
| `actionButtonBg` | NEXT / DONE button background |
| `navIconActiveBg` | Active side nav icon background |
| `navIconInactiveBg` | Inactive side nav icon background |
| `dividerColor` | Horizontal dividers throughout panels |
| `progressBarFill` | Video progress bar fill color |
| `progressBarBg` | Video progress bar track color |

`accentSecondary` is defined but has no runtime effect — it is used only in USS hover/active pseudo-class states which cannot be overridden at runtime.

### ImageData

| Field | Element |
|---|---|
| `startButtonBackground` | Welcome screen start button background |
| `homeTabButtonBackground` | Home screen large tab buttons |
| `topTabActiveBanner` | Active banner diagonal label |
| `iconHome/Product/Video/Info` | Side nav circle icons |
| `iconSpecsBackNavButton` | Back nav ghost button on Specs screen |
| `iconInspectBackNavButton` | Back nav ghost button on Inspect screen |
| `iconHide` | Hide UI utility button |
| `iconScreenshot` | Screenshot utility button |
| `iconFocus` | Inspect row focus buttons |
| `iconResetView` | Reset view icon label |
| `iconPrevNav / iconNextNav` | Prev / Next nav ghost buttons |
| `iconPlay/Pause/Mute/Unmute/Replay` | Video control buttons |
| `iconDownload` | Brochure download button |
| `iconClose` | Info overlay close button |

Leave any field as `""` to use the USS default.

---

## Typography System

Stylesheet.uss uses a three-layer class system. Every text element in UXML carries one class from each layer.

**Layer 1 — Font family** (applied by C# when a font asset is resolved from ThemeFontLibrary)
- `.font-body` — KumbhSans-Regular
- `.font-bold` — KumbhSans-ExtraBold, italic
- `.font-light` — KumbhSans-Light

**Layer 2 — Size tokens**
- `.text-xs` 10px · `.text-sm` 15px · `.text-md` 16px · `.text-base` 18px · `.text-title` 18px · `.text-lg` 20px · `.text-xl` 22px · `.text-2xl` 30px

`text-title` and `text-base` share the same default size (18px) but are separate classes so section title sizes and body sizes can be changed independently via `FontData.titleFontSize` and `FontData.bodyFontSize`.

**Layer 3 — Color tokens** (applied by C# from `ColorData`)
- `.text-primary` white · `.text-brand` off-white · `.text-accent` orange · `.text-muted` gray · `.text-dark` dark green

Structural classes (`.panel-section-title`, `.tab-button-active`, etc.) contain only layout properties — no font or color — so they never conflict with the typography layers.

---

## Navigation Logic

### Desktop product flow

```
Home → ProductSelection → [Specs] → [Inspect] → Home
```

The NEXT button on ProductSelection uses smart routing:
- If product has specs → go to Specs
- Else if product has inspect → go to Inspect
- Else NEXT button is hidden

The NEXT/DONE button on the Specs screen:
- If product has inspect → shows "NEXT", goes to Inspect
- If product has no inspect → shows "DONE", goes to Home

The BACK button on the Inspect screen:
- If product has specs → goes back to Specs, label shows specs section title
- If product has no specs → goes back to ProductSelection

### Top nav tab behavior

Tab buttons (CHOOSE PRODUCT / SPECS / INSPECT) are disabled and visually dimmed when the current product does not support that section. They remain visible to preserve the nav bar layout — only interaction is blocked. All three TopNav instances (one per product screen) are updated simultaneously.

### Mobile

On mobile the three product sections (Products, Specs, Inspect) all live inside a single scrollable HomeScreen. Navigation buttons are not used — sections show or hide based on product data.

---

## File Reference

### C# Scripts

| File | Purpose |
|---|---|
| `UINames.cs` | Central registry of all UXML element names and USS class names as constants |
| `ThemeData.cs` | Raw JSON-deserialisable data classes — `FontData`, `TextData`, `ColorData`, `ImageData` (hex strings and path strings) |
| `RuntimeThemeData.cs` | Resolved theme data — `RuntimeFontGroup`, `RuntimeColorGroup`, `RuntimeImageGroup` holding Unity Color, Font, Texture2D |
| `ThemeColorUtils.cs` | Static helper — parses `#RRGGBBAA` hex strings into Unity Color with fallback |
| `ThemeLoader.cs` | MonoBehaviour — fetches theme JSON and downloads all image assets from the CMS server. Reports progress via callback for the loading bar |
| `ThemeValidator.cs` | Validates parsed theme data before use |
| `ThemeFontLibrary.cs` | MonoBehaviour — holds font assets keyed by string; looked up by key at runtime |
| `HomeScreenThemeBootstrap.cs` | MonoBehaviour — selects desktop or mobile theme file and orchestrates the load |
| `HomeScreenThemeController.cs` | Plain C# class — manages boot coroutine, loading screen, error panel, progress bar. Always calls onComplete regardless of success or failure so the UITK input system fully initializes on every path |
| `HomeScreenUI.cs` | Caches all element references at startup including loading screen, error panel, and retry button; builds StyleTargets in one tree walk |
| `UIThemeApplicator.cs` | Applies `RuntimeThemeData` to cached refs — zero runtime queries. Guards against null theme internally |
| `HomeScreen.cs` | MonoBehaviour orchestrator — navigation, product selection, theme init. Always runs full ApplyThemeAndBuildControllers on both success and failure paths |
| `HomeScreenDisplayFlow.cs` | Screen show/hide logic for desktop and mobile |
| `HomeSceneModeController.cs` | Camera FOV, model rotation permission, decal visibility |
| `ScreenNavigator.cs` | Shows/hides screens, tracks current screen |
| `VideoUIController.cs` | Video playback state and progress bar update loop |
| `InspectUIController.cs` | Builds inspect point rows, manages active selection and camera |
| `SpecsUIController.cs` | Builds and populates all spec row types |
| `ProductSelectionUIController.cs` | Builds product buttons, manages selection highlight |
| `WelcomeScreenManager.cs` | Binds the start button on the welcome screen |
| `CameraRigBuilder.cs` | Builds and caches camera position arrays per product |
| `CameraController.cs` | Mouse and touch input — rotation and pinch-zoom |
| `ProductManager.cs` | Loads Product assets from Resources, manages instantiated prefabs |
| `Product.cs` | ScriptableObject data — specs, inspect points, brochure |
| `DecalController.cs` | Fades and scales the ground decal projector |
| `DeviceDetection.cs` | Detects desktop vs mobile, activates the correct UIDocument GameObject |
| `UIBlockerRaycast.cs` | Detects whether input is over UI to gate camera rotation |
| `ScreenshotHandler.cs` | Captures screen and sends to browser via JS interop |
| `WebGLDownloadManager.cs` | Constructs PDF URL and triggers browser download via JS interop |

### CMS Server Files

| File | Purpose |
|---|---|
| `server.js` | Express server — static file serving, theme save endpoint, image upload |
| `package.json` | Node.js dependencies — express, cors, multer |
| `themes/desktop-theme.json` | Desktop theme — all colors, text, font keys, image paths |
| `themes/mobile-theme.json` | Mobile theme — same structure, mobile-specific text variants |

### UXML Files

| File | Purpose |
|---|---|
| `0-Desktop-Instance.uxml` | Root desktop instance — assembles all desktop screens including loading screen and error panel |
| `0-Mobile-Instance.uxml` | Root mobile instance — assembles all mobile screens |
| `1-StartScreen_Desktop.uxml` | Desktop welcome screen |
| `1-StartScreen_Mobile.uxml` | Mobile welcome screen |
| `2-HomeScreen_Desktop.uxml` | Desktop home screen with large tab buttons |
| `2-HomeScreen_Mobile.uxml` | Mobile home screen with scrollable sections |
| `3-ProductSelection_Desktop.uxml` | Desktop product selection panel |
| `4-ProductSpecs_Desktop.uxml` | Desktop specs panel |
| `5-InspectProduct_Desktop.uxml` | Desktop inspect panel |
| `6-VideoScreen_Desktop.uxml` | Desktop video screen |
| `3-VideoScreen_Mobile.uxml` | Mobile video screen |
| `Shared_TopNav_Desktop.uxml` | Desktop top nav bar — used by all three product screens |
| `Shared_TopNav_Mobile.uxml` | Mobile top nav — used by Home and Video screens |
| `Shared_LeftPanel_Desktop.uxml` | Desktop left panel containing SideNav and Utils |
| `Shared_SideNav_Desktop.uxml` | Desktop circle icon navigation buttons |
| `Shared_Utils.uxml` | Hide, screenshot, logo — used in left panel and mobile |
| `Shared_InfoPanel.uxml` | Info overlay card — shared by desktop and mobile |
| `Shared_InspectFooter.uxml` | Prev/Next navigation row used by desktop and mobile inspect |
| `Shared_ResetView.uxml` | Reset view button with icon and label |
| `Shared_BrochureSection.uxml` | Download brochure row |
| `LoadingPanel.uxml` | Loading screen template — progress bar shown during theme boot |
| `ErrorPanel.uxml` | Error panel template — shown when theme fails to load, contains Retry button |
| `inspectRow.uxml` | Dynamically instantiated inspect point row template |
| `productButtonTemplate.uxml` | Dynamically instantiated product selection button |
| `specRow_Text.uxml` | Spec row — text value |
| `specRow_Bar.uxml` | Spec row — progress bar |
| `specRow_Toggle.uxml` | Spec row — toggle |
| `specRow_Chip.uxml` | Spec row — chip tags |

### Other Assets

| File | Purpose |
|---|---|
| `Stylesheet.uss` | All USS styles — typography system, layout, structural classes, loading and error panel styles |

---

## Known Constraints

**Hover and active pseudo-class states** cannot be driven by the theme. `.circle-icon:hover`, `.inspect-button:hover`, `.inspect-button.active`, `.selected-button.active` are defined only in USS and are not overridable at runtime via C#.

**`accentSecondary`** in ColorData has no runtime effect. It documents the blue used for selected inspect states in USS hover/active rules only.

**`panelCardBg` on mobile theme** has alpha 0 by design — mobile panels have no visible background card since they scroll over the 3D view.

**`UIBlockerRaycast.uiDocument`** must be assigned in the Inspector and must reference the active UIDocument (desktop or mobile depending on current mode).

**UITK input system requires full initialization.** Buttons inside template instances only become clickable after the UITK panel has completed a full style and layout pass triggered by `UIThemeApplicator.Apply()` and `BindAndRefreshUI()`. For this reason `HomeScreenThemeController` always calls `OnThemeReady` regardless of whether the theme loaded or not, and `HomeScreen.OnThemeReady` always runs `ApplyThemeAndBuildControllers()` on both success and failure paths. The error panel retry button is bound through the same `BindButtons(actions)` path as all other UI buttons.

**`ErrorPanel.uxml` must reference `Stylesheet.uss`** via a `<Style>` tag at the top of the file. Without this, USS classes do not apply and the panel may not render or receive input correctly.

**Loading screen element names** — `LoadingScreen` is the instance wrapper in `0-Desktop-Instance.uxml`. `Loading_Bar_Fill` is the inner progress fill element queried from root. Both are cached in `HomeScreenUI` and driven by `HomeScreenThemeController`.

---

## WebGL Deployment Notes

Two JavaScript functions must be defined in the WebGL template plugin:

```javascript
function VShowroom_DownloadScreenshot(base64String) { ... }
function VShowroom_DownloadPDFUrl(url, filename) { ... }
```

Without these, screenshot capture and PDF download will throw runtime errors on WebGL builds. In the Editor, `Application.OpenURL` is used as a fallback for PDF download.

PDF brochure files must be accessible at `WebGLDownloadManager.pdfBaseUrl + "/" + product.brochurePdfFile`. Update `pdfBaseUrl` in the Inspector to point to your server.

Mobile detection on WebGL uses a screen width threshold of 900px. Adjust `DeviceDetection.DetectMobile` if a different breakpoint is needed.

For production, update `ThemeLoader.cmsBaseUrl` in the Inspector to point to your live server. The server runs identically in production — swap `localhost:3000` for your domain.
