---
name: skynet-framework
description: >-
  Architecture and conventions of the SkyNet web framework (ASP.NET Core / .NET 10,
  SQL Server, vanilla JavaScript, no SPA/build step) that underpins the ServiceNet,
  SalesNet, and BizJournal products. Use this whenever writing, reading, reviewing,
  or debugging any page built on SkyNet — the .cs page class, its .js/.css/.html
  companions, or its SQL. It covers the self-contained per-page model, the
  skynet.core.js "$" client engine and its WebAct DOM-operation opcodes, the server
  ApiResponse / SQLData / WebBase / Helper model, the XFN permission system and its
  XYS* tables, CS→JS variable transfer, and application.cfg configuration. Reach for
  this skill even when the request only mentions a single page, a method, an opcode,
  a permission grant, or "the $ call" — anything on SkyNet touches these conventions.
---

# SkyNet Framework

SkyNet is a **server-driven** web framework: ASP.NET Core / .NET 10 on the back end,
SQL Server for data, and **vanilla JavaScript** on the client with **no SPA and no
build step**. The defining idea: an interaction never returns a new HTML page. The
browser POSTs to a page method; the method returns a **JSON list of DOM operations**;
a small client engine (`skynet.core.js`) applies them in place. The server owns the
DOM and pushes surgical mutations to it.

---

## 1. Page anatomy — self-contained, four files

Every page is four co-located files sharing a base name (e.g. `Report_Financial`):

| File            | Holds                                                             |
|-----------------|------------------------------------------------------------------|
| `Name.cs`       | The page class (`: WebBase`), its DTOs, and its server methods   |
| `Name.js`       | One IIFE namespace `NameJs = (function(){ ... })()` + `reveal()` |
| `Name.css`      | Styles, every selector under a **short page-unique prefix**      |
| `Name.html`     | Markup with `{placeholder}` tokens the `.cs` replaces            |

Rules that hold across the framework:

- **Self-contained, duplicate-don't-abstract.** A page owns its own CSS prefix, its
  own JS IIFE, and its own C# methods/DTOs. Prefer copying a block into a second page
  over introducing a shared helper. The **only** cross-page C# sharing is `*Model.cs`
  files (plain DTO/enum holders in a `Models` namespace).
- **Prefix discipline.** CSS classes and ids are prefixed per page (`finrp-`, `cfgms-`,
  …) so pages never collide when several are open at once.
- **JS namespace.** The page script is a single IIFE assigned to `NameJs`, exposing a
  small surface (typically `reveal` plus event handlers) via its return object; it
  ends by calling `NameJs.reveal()`.
- **Placeholders.** The `.cs` builds HTML fragments and injects them with
  `HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText.Replace("{token}", built)`.
- Assets are served from **root folders** (`scripts/`, `styles/`, `images/`,
  `photos/`, `htmls/`), not `wwwroot`.

---

## 2. Request lifecycle (one round trip)

1. A user action calls `$('Method')`, `$Call('Method', 'k=v&...')`, or an opcode
   re-invokes a call. Names are relative to the current page.
2. The client POSTs **FormData** to `<area>/<Page>/<Method>` (URI derived from the
   current path by `$appPath`).
3. `WebBase.OnInit` authenticates; the framework routes to the page method; the
   method returns an **`ApiResponse`**.
4. The response body is JSON: `{ "data": [ {o,k,p1,p2}, ... ] }` — a list of DOM ops.
5. The client's `$ApiRequestSuccess` → `$WebAct(JSON.parse(...))` applies each op.

If the response body begins with `<!DOCTYPE html>` the client does a full
`document.write` (used for hard navigations / login redirects).

---

## 3. Client engine — `scripts/skynet.core.js`

Minified as `scripts/skynet.core.min.js`. Everything is exposed as global `$`-prefixed
functions. **The framework introspects function names at runtime** (`$fn`/`$fnr` read
`arguments.callee.caller.toString()`), and inline HTML `onclick`s call these globals by
name — so when minifying, **never mangle identifiers or convert to arrow functions**.

### Entry points

- **`$(f, d)`** — POST to method `f` with payload `d` (object → `JSON.stringify`). If
  `f` is null/`this`, the *calling function's own name* is used (so a handler can call
  `$()` to invoke its server-side namesake).
- **`$Call(f, args)`** — `args` is a query-style string `"k=v&k2=v2"`. Special RHS
  tokens are resolved client-side before sending: `::` → element value by id, `:::` →
  checked values by name. Shows the wait overlay, then posts.
- **`$ApiRequest(func, data, onSuccess, onError)`** — the transport. Builds the URI,
  assembles the envelope, sends an `XMLHttpRequest` POST, routes the response to
  `$WebAct`.
- **`$$` / `$$KeyEvent`** — the *type-qualified* variants (a call carrying an explicit
  page/handler type alongside the method), used where the target isn't the current
  page's own method.

### The POST envelope (always sent)

`$ApiRequest` appends, on every call:

- `#data` — the JSON payload
- `#offset` — `new Date().getTimezoneOffset()`
- `#time` — `$getDT()` (local `YYYY-M-D H:M:S`)
- `#tzn` — `$getTZN()` (IANA-ish zone label from the Date string)
- **all URL query params** (via `$UrlParams`)
- **all page-relevant localStorage** (via `$getLocalData`)
- **all form field values** (via `$getElmValues`): every `input`/`select`/`textarea`/
  `button` keyed by `id || name`; checkboxes/radios only when checked (value, or `'1'`
  for a bare `on`); `file` inputs append their `File` objects.

So server methods can read any on-screen field or stored value without it being named
explicitly in `#data`.

### localStorage scoping

Keys are namespaced. A key prefixed `#global.` is global; otherwise it's implicitly
scoped to the current page (`<pagename>.<key>`). `$getLocalData` transmits global keys
as-is and strips the page prefix from page-scoped keys. Set/clear via opcodes 90–92 or
`$` helpers.

### Overlays, popups, modals

- **`$WaitOn()` / `$WaitOff()`** — the global spinner overlay (`#loading-overlay`).
- **`$PopUp(html, bh)`** — raw positioned popup (auto-fits within the viewport).
- **`$PopOn(body, target)`** — centered card popup; `body` may be **base64** (auto-
  detected via `$isBase64` → `atob`); optionally disables pointer events on `target`.
- **`$PopOff()`** — closes the current page's popup (`$ppId()` = per-page popup id).
- **`$ModalOn(title, content)` / `$ModalOff()`** — the shared `modal_overlay`; title
  and content are base64-aware.

---

## 4. WebAct opcode reference (server → DOM)

`$WebAct` switches on `o` (the opcode) for each `{o,k,p1,p2}`. `k` is the target
(id, name, url, or data-elmid); `p1`/`p2` are payload/args. The `x9` id-form and its
`x0`+ name-form are paired (act on one element by id vs. all elements by `name`).

| `o`   | Action                               | k / p1 / p2                          |
|-------|--------------------------------------|--------------------------------------|
| 0     | no-op                                | —                                    |
| 1 / 2 | `body.innerHTML` set / append        | p1 = html                            |
| 19 / 49 | set **value** by id / name         | p1 = value                           |
| 20 / 50 | set **innerHTML** by id / name     | p1 = html                            |
| 21 / 51 | append inner/value by id / name    | p1 = html                            |
| 22 / 52 | clear by id / name                 | —                                    |
| 10 / 40 | `setAttribute` by id / name        | p1 = attr, p2 = value                |
| 11 / 41 | `removeAttribute` by id / name     | p1 = attr                            |
| 25 / 55 | `style.setProperty` by id / name   | p1 = prop, p2 = value                |
| 26 / 56 | `style.removeProperty` by id / name| p1 = prop                            |
| 30 / 60 | replace `outerHTML` by id / name   | p1 = html                            |
| 31 / 61 | remove element by id / name        | —                                    |
| 38    | write into `<iframe>` by id          | p1 = html (open/write/close)         |
| 70    | text find/replace within a subtree   | k = root id (or body), p1 find, p2 repl |
| 75    | toggle a table detail row            | k = trigger id, p1 = row html        |
| 110 / 111 | data-elmid set / remove attribute| k = data-elmid, p1 attr, p2 val      |
| 120 / 121 / 122 | data-elmid set / append / clear inner | k = data-elmid, p1 html    |
| 125 / 126 | data-elmid style set / remove     | k = data-elmid, p1 prop, p2 val      |
| 76 / 77 | append / remove `<script src>`     | k = url, p1 = target id (append)     |
| 78 / 79 | append / remove `<link href>`      | k = url                              |
| 80    | run inline script                    | p1 = JS source (nonce-wrapped, then removed) |
| 87    | call API `$(k, p1)`                  | k = method, p1 = data                |
| 88    | call API `$$(k, p1, p2)`             | typed call                           |
| 90 / 91 / 92 | localStorage set / clear / remove | k = key, p1 = value                |
| 93 / 94 | cookie set / remove                | k, p1 = value, p2 = seconds (`''`=persistent; empty p1 on 93 also clears) |
| 95 / 96 / 97 | sessionStorage set / clear / remove | k = key, p1 = value             |
| 195   | modal on → `$ModalOn(k, p1)`         | k = title, p1 = content              |
| 294   | `$PopUp(p1, p2)`                     | p1 = html, p2 = behavior flag        |
| 295   | `$PopOn(p1, k)`                      | p1 = body, k = target                |
| 296   | `alert(p1)`                          | p1 = message                         |
| 297   | navigate (clears body, sets href)    | k = url, p1 = query params           |
| 298   | `$Navigate2(k, p1)`                  | k = url, p1 = data                   |
| 299   | `window.open(k, ...)`                | k = url, p1 = `'N'`→same tab else `_blank` |
| 200   | vertical-center an element           | k = element id                       |

The server never hand-writes this array — it uses `ApiResponse` builder methods
(next section), each of which pushes one op.

---

## 5. Server — `ApiResponse`

A page method returns an `ApiResponse`; its `data` list is the opcode array above.
Build it with the typed helpers rather than raw ops. The common ones (each maps 1:1 to
an opcode):

- `SetElementContents(id, html)` → set innerHTML (20)
- `SetElementValue(id, value)` → set value (19)
- `AppendElementContents(id, html)` / `ClearElement(id)` → (21 / 22)
- `SetAttribute(id, attr, val)` / `RemoveAttribute(id, attr)` → (10 / 11)
- `SetStyle(id, prop, val)` / `RemoveStyle(id, prop)` → (25 / 26)
- `ExecuteScript(js)` → run inline script (80) — the workhorse for calling a page's
  JS after a server action, e.g. `response.ExecuteScript("NameJs.reloadTab();")`
- `ModalWindow(title, content)` → (195); `Alert(msg)` → (296)
- `Navigate(url, data)` / `OpenWindow(url, ...)` → (297 / 299)
- `DownloadFile(...)` → hands the browser a temp download link
- `SetVariableData<T>(name, obj)` → ships a typed object to the client (see §8)

Standard error surfacing inside a method:

```csharp
var (dt, emsg) = await Helper.GetDataTable(sql, parms);
if (!string.IsNullOrEmpty(emsg)) { response.ModalWindow(Helper.IssueFound, Helper.ErrContentHtml(emsg)); return response; }
```

---

## 6. Server — `SQLData`

Data access is the `SQLData` class (namespace `SkyNet`). It reads connection info from
config (`SQLInfo`) — no null concerns.

- **Async:** `GetDataAsync`, `GetDataSetAsync`, `PutDataAsync`.
- **Sync:** `SQLDataTable` (static; returns a `(DataTable dt, string rlt)` tuple),
  `DataGetSet` (instance; takes a `SQLInfo`).
- Private `ErrorPrefix = "Error:"`.
- **Low-level write that bypasses the Helper archive lock:**
  `await new SQLData().PutDataAsync(List<string> sqls, List<SqlParameter> parms)`
  returns `""` on success or the error text on failure — same path `Helper.PutData`
  uses internally. Use this deliberately when a write must succeed even in archive
  (read-only) mode; otherwise prefer `Helper.PutData`.

Most reads go through `Helper.GetDataTable(sql, parms)` → `(DataTable, string emsg)`.
Always parameterize with `SqlParameter`.

---

## 7. Server — `WebBase` and `Helper`

**`WebBase : WebPage`** is the page base class.

- `OnInit` authenticates via `Helper.GetAuthData()` (a role-less / unauthenticated
  caller is stopped here).
- `OnInitialized` is where a page loads its data, builds fragments, and injects them
  into `HtmlDoc.HtmlBodyText`.
- Google translation is handled centrally in `OnAfterRender` + `OnResponse`, targeting
  `SetElementContents` and server page-method actions.

**`Helper`** — the server-side utility surface. Commonly used:

- `Helper.GetAuthData()` — current user / session.
- `Helper.GetDataTable(sql, parms)` — parameterized read → `(DataTable, string emsg)`.
- `Helper.GetTabActions(parms)` — the current user's **granted** tab-actions (see §9).
- `Helper.PutData(...)` — the standard write path; **hard-blocks all writes when the
  app run mode is Archived (2)**, returning a "locked in archive mode" message.
- `Helper.AppRunMode()` — the current run mode as an int (0 Initialize, 1 Normal,
  2 Archived).
- `Helper.GetWebEnv("area.key")` — read a config value (see §10).
- `Helper.IssueFound`, `Helper.ErrContentHtml(text)` — standard error modal pieces.
- CSV/download helpers (`CreateCsv`, `TempFolder`, `DownLoadFileLink`) used by export
  methods together with `response.DownloadFile`.

---

## 8. CS → JS data transfer

Preferred, current pattern for handing structured data to a page's JS:

- **Server:** `response.SetVariableData<T>(name, obj)` — serializes `obj` and ships it.
- **Client:** `var v = $GetVarData(name)` — retrieves it. Under the hood
  `$SetVarData(name, b64)` does `JSON.parse(decodeURIComponent(escape(atob(b64))))`
  into `window.$VarData`; `$GetVarData(name, remove=true)` returns the value and
  **deletes it by default** (pass `false` to keep it).

Legacy pages that embed a JSON blob inline in the HTML are **left in place** — do not
retrofit them to the variable-data mechanism.

---

## 9. Permissions & navigation (XFN + XYS* tables)

Method-level authorization runs through the SQL function
**`XFN_UserMethodPermission(@userid, @type, @method)`** on every page-method call, in
the native/IIS layer before the managed handler:

- A method that is **not registered** as any action's method is **permissive**
  (callable by anyone with page access).
- A method that **is registered** requires the caller to hold **all three grants**
  (role→page, role→tab, role→tab-action).
- A user with **no role** is blocked entirely.

**Navigation / action tables (`XYS*`):**

- `XYSPAGE` — `PAGE_ID, PAGE_AREA, PAGE_SORT, …`
- `XYSPAGETAB` — `TAB_ID, PAGE_ID, TAB_NAME, …`
- `XYSTABACTION` — `TABACTION_ID, TAB_ID, TABACTION_NAME, TABACTION_LABEL,
  TABACTION_METHOD, SYSDTE, SYSUSR`
- `XYSROLE` — `ROLE_ID, ROLE_NAME`
- `XYSROLEPAGE`, `XYSROLETAB`, `XYSROLETABACTION` — the three grant tables
  (`XYSROLETABACTION` = `ROLETABACTION_ID, ROLE_ID, TABACTION_ID, SYSDTE, SYSUSR`)

**Two conventions that are easy to get wrong:**

1. **Action-name composition.** `Helper.GetTabActions` returns each action's
   `TABACTION` as the **dotted path** `PAGE_AREA + '.' + TAB_NAME + '.' +
   TABACTION_NAME`. So a permission check compares
   `x.TABACTION == "Area.Tab.ActionName"`, while the stored `TABACTION_NAME` is just
   the **short** `ActionName`. (Compose permission keys as
   `PAGE_AREA.TAB_NAME.TABACTION_NAME`; store only the short name in the row.)
2. **Method matching is a comma-joined LIKE.** `TABACTION_METHOD` may hold **several**
   method names joined by commas, and `XFN_UserMethodPermission` matches `@method`
   against that list. The idiom: a data/view method (`GetX`) gets its **own** action,
   but all **`DownloadX` / export methods are pooled under a single `Export` action**
   whose `TABACTION_METHOD` is the comma-joined list of every download method. When you
   add a new export, **append its method to the `Export` action's list** — registering
   only the `Get` action leaves the download unauthorized (or, on a permissive setup,
   inconsistently gated).

**Registration SQL is idempotent:** look up `PAGE_ID` by `PAGE_AREA`, `TAB_ID` by
`TAB_NAME`, then `INSERT ... WHERE NOT EXISTS`; grant by inserting `XYSROLETABACTION`
rows (commonly copied from an existing peer action so the same roles see the new one).

**Page-side gate for exports:** a download method typically also checks
`HasAction("Area.Tab.Export")` in code before streaming.

---

## 10. Configuration & hosting

- **`application.cfg`** (under the app-config folder) is **pipe-delimited**:
  `app.app.name | ServiceNet`, one `key | value` per line, with a `System` and a
  `Custom` section. Read any value with `Helper.GetWebEnv("app.sqldb.catalog")` /
  `GetWebEnv("user.sso.uri")`. Never expose this folder over HTTP.
- **Run mode** is a separate switch stored in the DB (`XYSOPTION`, `ORIGINE/RunMode`),
  not the `.cfg` value — `Helper.AppRunMode()` reads it; Archived (2) makes the whole
  app read-only via `Helper.PutData`.
- **Hosting:** in-process ASP.NET Core Module (IIS), static assets served from the
  **content-root folders**. Because the whole content root is served, harden
  `web.config` `requestFiltering`: `hiddenSegments` for server-internal folders
  (config, htmls, data, logs) **plus** a `fileExtensions` deny-list
  (`.config .json .dll .exe .pdb .cfg`) so root files (appsettings, assemblies) can't
  be downloaded. Never block `.js`/`.css`/image extensions — the client fetches those
  by URL and blocking them renders pages unstyled.

---

## Quick gotchas

- Server pushes DOM ops; it does **not** return pages. Think in `ApiResponse` builder
  calls, not HTML responses.
- After a server mutation, re-run page JS with `ExecuteScript("NameJs.xxx();")`.
- Minifying `skynet.core.js`: preserve every function name and global — the engine
  reads function names at runtime and HTML calls `$`-globals by name.
- New export? Register the `Get` action **and** add the download method to the shared
  `Export` action's comma-joined `TABACTION_METHOD`.
- `TABACTION_NAME` stores the short name; permission keys are the dotted
  `PAGE_AREA.TAB_NAME.TABACTION_NAME`.
- Writes are blocked in Archived mode via `Helper.PutData`; go through
  `new SQLData().PutDataAsync(...)` only when a write must bypass that.
