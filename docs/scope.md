# GeoCRS Transform — v1 Scope and Policy

## 1. Supported in v1

### 1.1 Geographic CRS transforms
- Latitude/longitude (and optional height) transforms for a **curated set of major datums**.
- Transform paths: **Geographic ↔ Geographic** via datum shift (Helmert 7-param, optionally 3-param) and ellipsoid changes.
- WGS84 is the **hub datum**; all geographic transforms route through WGS84.

### 1.2 Projected CRS transforms
- Eastings/northings (meters) for a **curated set of major projections**:
  - Web Mercator (EPSG:3857)
  - British National Grid (EPSG:27700)
  - UTM (parametric: zone + hemisphere; aliases e.g. UTM30N, UTM33N)
- Transform paths:
  - **Geographic ↔ Projected** (projection forward/inverse)
  - **Projected ↔ Projected** (inverse projection → datum shift → forward projection)

### 1.3 Identifiers
- Canonical CRS identifiers via **EPSG:&lt;code&gt;** (e.g. EPSG:4326, EPSG:27700).
- **Friendly aliases** (e.g. WGS84, BNG, UTM30N) resolved through the catalog.

---

## 2. Not guaranteed in v1

- **Survey-grade transforms** where **grid shifts** (NTv2, NADCON, etc.) are required. The library uses Helmert-only datum shifts; accuracy may be reduced where grids would normally apply.
- **Coverage for “all EPSG”** — only a curated subset of major CRSs is included.
- **Automatic “best operation by area of use”** selection; a single default transform per CRS pair is used. Area-based selection may be added in a future version.

---

## 3. Accuracy policy

### 3.1 Accuracy classes
Every transform result includes an **AccuracyClass**:

| Class    | Meaning |
| -------- | ------- |
| **High** | Parameters from authoritative source; Helmert/projection only; no grid expected for typical use. |
| **Medium** | Parameters well-defined but grid shifts are commonly used in practice (e.g. OSGB36, NAD83). |
| **Low** | Approximate or regional parameters; use for indication only. |
| **Unknown** | No accuracy metadata; use with caution. |

### 3.2 Warnings
`TransformResult` includes a list of **warnings** when:
- Parameters are approximate or from a single regional source.
- Grid shifts would normally be applied (e.g. British National Grid, NAD83).
- The CRS is not supported (when not in Strict mode).
- Axis order or other conventions differ from default.

### 3.3 Result contract
- **TransformResult&lt;T&gt;** must expose:
  - `Output` (the transformed coordinate)
  - `Warnings` (read-only list of warning messages)
  - `AccuracyClass`
  - `TransformPath` (e.g. `EPSG:27700 → EPSG:4326 → EPSG:3857`)

---

## 4. Testing policy

- **Every implementation step** ends with `dotnet test`; the test suite must pass before proceeding.
- **Cross-platform CI** (Windows, Ubuntu, macOS) must remain green.
- **No flaky tests**: any randomness (e.g. property-style round-trip tests) must use **seeded** RNG only.
- **Tier A tests** (always-on): unit tests (parsing, validation, catalog, ECEF, Helmert, projections), integration tests (representative CRS pairs), and property-style round-trip tests within tolerance by accuracy class.
- **Tier B tests** (optional): PROJ comparison via env-gated script (e.g. `VERIFY_WITH_PROJ=1`); not required in CI.

---

## 5. Dependency constraint

- **No external or native dependencies** for the core library (v1).
- Standalone NuGet package; embedded CRS data (e.g. `major_crs.json`) only.
