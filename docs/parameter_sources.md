# GeoCRS Transform — Parameter sources and accuracy notes

This document records the source of datum and projection parameters for each CRS included in the library, and notes on expected accuracy. It is updated as CRSs are added.

---

## Conventions

- **Helmert 7-parameter**: Translations (tx, ty, tz) in **meters**; rotations (rx, ry, rz) in **arc-seconds** (converted to radians internally); scale in **parts per million (ppm)**.
- **ToWgs84**: Transform from this CRS to WGS84 (ECEF). **FromWgs84**: inverse (WGS84 → this CRS).
- When multiple regional or time-dependent transformations exist, v1 uses a **single default**; the choice is documented here.

---

## Geographic CRSs (datum + ellipsoid)

| CRS | EPSG | Ellipsoid | ToWgs84 / FromWgs84 source | Accuracy note |
| ----- | ------ | --------- | --------------------------- | -------------- |
| WGS 84 | 4326 | WGS84 | Identity (hub) | High |
| ETRS89 | 4258 | WGS84 | Identity | High |
| OSGB 1936 | 4277 | Airy 1830 | EPSG:1314 (Helmert 7-param); FromWgs84 = inverse of ToWgs84 | Medium; grid (OSTN) not applied |

---

## Projected CRSs (projection + base geographic CRS)

| CRS | EPSG | Base geographic | Projection params source | Accuracy note |
| ----- | ------ | ---------------- | ------------------------- | -------------- |
| WGS 84 / Pseudo-Mercator | 3857 | EPSG:4326 | Standard Web Mercator (sphere 6378137 m) | High |
| British National Grid | 27700 | EPSG:4277 | OSGB TM: CM -2°, lat origin 49°, k0 0.9996012717, FE 400000, FN -100000 | Medium; grid not applied |
| WGS 84 / UTM zone 30N | 32630 | EPSG:4326 | UTM: CM -3°, k0 0.9996, FE 500000, FN 0 | High |
| WGS 84 / UTM zone 31N | 32631 | EPSG:4326 | UTM: CM 3°, k0 0.9996, FE 500000 | High |
| WGS 84 / UTM zone 33N | 32633 | EPSG:4326 | UTM: CM 15°, k0 0.9996, FE 500000 | High |

---

## References (to be cited)

- EPSG Geodetic Parameter Dataset (e.g. epsg.io, IOGP)
- National mapping agency technical notes (e.g. OSGB, NGA)
- PROJ transformation parameter database (for comparison only; this library does not depend on PROJ at runtime)
