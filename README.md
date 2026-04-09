# Product Configurator — Developer Reference

## Overview

Unity WebGL product configurator built entirely with Unity UI Toolkit (UITK). Supports desktop and mobile layouts from a single codebase, switching at runtime based on screen width. All UI text, colors, fonts, and icons are controlled through ScriptableObject theme assets with no code changes required.

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
- `desktopTheme`, `mobileTheme` — ConfiguratorUITheme ScriptableObject assets
- `video_FOV`, `normal_FOV` — camera field of view values
- `resourcesPath` — path inside Resources folder where Product assets are stored (default: `GamyraDrive`)
- `defaultProductId` — product ID to select on startup

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

## Adding a Product

1. Right-click in the Project panel → Create → Product Configurator → Product
2. Fill in `productName` and `productId` — the ID must be unique across all products
3. Assign `productPrefab`
4. Toggle `showSpecs` and `showInspect` to control which panels appear for this product
5. Add `SpecRow` entries under Specs — choose the type (Text, Bar, InvertedBar, Toggle, Chips) and fill the relevant fields
6. Add `InspectPoint` entries — each needs a label and a `cameraAnchor` Transform in the scene
7. Optionally fill `brochurePdfFile` (filename relative to `pdfBaseUrl`) and `brochureDownloadName`
8. Place the Product asset inside the Resources folder at the path matching `resourcesPath` on HomeScreen

Products are loaded automatically at startup via `Resources.LoadAll` and sorted by `productId`.

---

## UI Architecture

Four layers with strict one-way dependencies:

```
ConfiguratorUITheme  (ScriptableObject — all customisable data)
        ↓
UIThemeApplicator    (reads theme, writes to cached element refs)
        ↓
HomeScreenUI         (caches all element refs + StyleTargets at startup)
        ↓
HomeScreen           (MonoBehaviour — orchestrates everything)
```

**`UINames.cs`** sits outside this chain as a shared registry. Every UXML element name and USS class name used in code is defined here as a constant. No script contains a hardcoded UI string.

### Startup sequence

1. `HomeScreen.OnEnable` creates `HomeScreenUI` — walks the visual tree once, caches all element references and builds `StyleTargets` lists
2. Creates `UIThemeApplicator` and calls `Apply(theme)` — writes all theme values to the cached refs with no further queries
3. Remaining modules (`ScreenNavigator`, `VideoUIController`, etc.) are initialised with pre-cached refs
4. Buttons are bound via the actions dictionary — all keys are `UINames` constants

---

## Customising the UI

Open `Assets/UITK/Themes/DesktopTheme.asset` or `MobileTheme.asset` in the Inspector.

### FontGroup

| Field | Controls | Default |
|---|---|---|
| `bodyFont` | KumbhSans-Regular — body text, buttons, labels | KumbhSans-Regular |
| `boldFont` | KumbhSans-ExtraBold — logos | KumbhSans-ExtraBold |
| `lightFont` | KumbhSans-Light — welcome description | KumbhSans-Light |
| `bodyFontSize` | Body text size | 18px |
| `boldFontSize` | Logo text size | 30px |
| `tabFontSize` | Tab button text size | 15px |
| `titleFontSize` | Panel section title size | 18px |
| `buttonFontSize` | Action button text size | 16px |
| `smallFontSize` | Reset view label size | 10px |

Font assets must be assigned in the Inspector. Sizes only override USS when changed from their defaults.

### TextGroup

Every visible string in the UI. Welcome screen text, tab labels, button labels, section titles, navigation labels, info overlay body text. Desktop and mobile info overlay bodies are separate fields.

### ColorGroup

| Field | Controls |
|---|---|
| `accentPrimary` | Active tab color, active nav icon background |
| `brandLogoColor` | Logo labels, welcome title |
| `primaryText` | All body labels |
| `secondaryText` | Inactive tabs, separators |
| `actionButtonText` | Text on NEXT / DONE buttons |
| `welcomeBg` | Welcome screen background |
| `topNavBg` | Desktop top nav bar background |
| `mobileNavBg` | Mobile top nav bar background |
| `panelCardBg` | Desktop panel card background |
| `overlayBackdropBg` | Info overlay backdrop |
| `infoCardBg` | Info overlay card background |
| `actionButtonBg` | NEXT / DONE button background |
| `navIconActiveBg` | Active side nav icon background |
| `navIconInactiveBg` | Inactive side nav icon background |
| `dividerColor` | Horizontal dividers throughout panels |
| `progressBarFill` | Video progress bar fill color |
| `progressBarBg` | Video progress bar track color |

`accentSecondary` is defined but has no runtime effect — it is used only in USS hover/active pseudo-class states which cannot be overridden at runtime.

### ImageGroup

| Field | Element |
|---|---|
| `startButtonBackground` | Welcome screen start button background |
| `homeTabButtonBackground` | Home screen large tab buttons |
| `topTabActiveBanner` | Active banner diagonal label |
| `iconHome/Product/Video/Info` | Side nav circle icons |
| `iconViewSpecsButton` | Back nav ghost button on Specs screen |
| `iconInspectButton` | Back nav ghost button on Inspect screen |
| `iconHide` | Hide UI utility button |
| `iconScreenshot` | Screenshot utility button |
| `iconFocus` | Inspect row focus buttons |
| `iconResetView` | Reset view icon label |
| `iconPrevNav / iconNextNav` | Prev / Next nav ghost buttons |
| `iconPlay/Pause/Mute/Unmute/Replay` | Video control buttons |
| `iconDownload` | Brochure download button |
| `iconClose` | Info overlay close button |

Leave any field null to use the USS default.

---

## Typography System

Stylesheet.uss uses a three-layer class system. Every text element in UXML carries one class from each layer.

**Layer 1 — Font family** (applied by C# when a font asset is assigned in the theme)
- `.font-body` — KumbhSans-Regular
- `.font-bold` — KumbhSans-ExtraBold, italic
- `.font-light` — KumbhSans-Light

**Layer 2 — Size tokens**
- `.text-xs` 10px · `.text-sm` 15px · `.text-md` 16px · `.text-base` 18px · `.text-title` 18px · `.text-lg` 20px · `.text-xl` 22px · `.text-2xl` 30px

`text-title` and `text-base` share the same default size (18px) but are separate classes so section title sizes and body sizes can be changed independently via `FontGroup.titleFontSize` and `FontGroup.bodyFontSize`.

**Layer 3 — Color tokens** (applied by C# from `ColorGroup`)
- `.text-primary` white · `.text-brand` off-white · `.text-accent` orange · `.text-muted` gray · `.text-dark` dark green

Structural classes (`.panel-section-title`, `.tab-button-active`, etc.) contain only layout properties — no font or color — so they never conflict with the typography layers.

**Legacy stubs** — `.KS-regular-text`, `.KS-extrabold-text`, `.KS-regular-text-14px` remain as empty classes in USS to prevent errors from any UXML elements that still reference them. They can be removed from USS and UXML once fully migrated.

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

## Adding a New Theme

1. Right-click in Project → Create → Product Configurator → UI Theme
2. Name it and configure all four groups in the Inspector
3. Assign font assets in FontGroup — drag `.ttf` files from `Assets/UITK/Fonts/`
4. Assign the new theme to `HomeScreen.desktopTheme` or `HomeScreen.mobileTheme`

Both DesktopTheme and MobileTheme must always be assigned. If `mobileTheme` is null, the mobile layout will have no theme applied.

---

## File Reference

### C# Scripts

| File | Purpose |
|---|---|
| `UINames.cs` | Central registry of all UXML element names and USS class names as constants |
| `ConfiguratorUITheme.cs` | ScriptableObject — FontGroup, TextGroup, ColorGroup, ImageGroup |
| `HomeScreenUI.cs` | Caches all element references at startup; builds StyleTargets in one tree walk |
| `UIThemeApplicator.cs` | Applies theme to cached refs — zero runtime queries |
| `HomeScreen.cs` | MonoBehaviour orchestrator — navigation, product selection, theme init |
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

### UXML Files

| File | Purpose |
|---|---|
| `0-Desktop-Instance.uxml` | Root desktop instance — assembles all desktop screens |
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
| `inspectRow.uxml` | Dynamically instantiated inspect point row template |
| `productButtonTemplate.uxml` | Dynamically instantiated product selection button |
| `specRow_Text.uxml` | Spec row — text value |
| `specRow_Bar.uxml` | Spec row — progress bar |
| `specRow_Toggle.uxml` | Spec row — toggle |
| `specRow_Chip.uxml` | Spec row — chip tags |

### Other Assets

| File | Purpose |
|---|---|
| `Stylesheet.uss` | All USS styles — typography system, layout, structural classes |
| `DesktopTheme.asset` | Default desktop theme — all fonts, colors, icons assigned |
| `MobileTheme.asset` | Default mobile theme — same as desktop with transparent panel card background |

---

## Known Constraints

**Hover and active pseudo-class states** cannot be driven by the theme. `.circle-icon:hover`, `.inspect-button:hover`, `.inspect-button.active`, `.selected-button.active` are defined only in USS and are not overridable at runtime via C#.

**`accentSecondary`** in ColorGroup has no runtime effect. It documents the blue used for selected inspect states in USS hover/active rules only.

**`panelCardBg` on MobileTheme** has alpha 0 by design — mobile panels have no visible background card since they scroll over the 3D view.

**`UIBlockerRaycast.uiDocument`** must be assigned in the Inspector and must reference the active UIDocument (desktop or mobile depending on current mode).

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

