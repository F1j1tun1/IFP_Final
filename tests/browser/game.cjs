const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { chromium, firefox } = require("playwright");
const { serve } = require("./server.cjs");
const mechanics = require("./mechanics.cjs");
const url = process.env.GAME_URL || "http://127.0.0.1:5080/IFP_Final/";
const key = "thornwatch.high-score.v1",
    runKey = "thornwatch.run.v2";
const screenshots = path.resolve("artifacts/screenshots");
const report = [],
    failures = [];
fs.mkdirSync(screenshots, { recursive: true });
const button = (p, n) =>
    p.getByRole("button", { name: n, exact: typeof n === "string" });
const pad = (p, id) => p.locator(`.pad[aria-label^="Pad ${id}:"]`);
const number = async (p, id) =>
    Number((await p.getByTestId(id).innerText()).replace(/[^0-9]/g, ""));
const shot = (p, name) =>
    p.screenshot({
        path: path.join(screenshots, name + ".png"),
        fullPage: true,
    });
async function ready(p) {
    await p.goto(url);
    await button(p, "New journey →").waitFor({ timeout: 60000 });
}
async function start(p) {
    await button(p, "New journey →").click();
}
async function text(p, selector, value) {
    await p.waitForFunction(
        ({ selector, value }) =>
            document.querySelector(selector)?.textContent.includes(value),
        { selector, value },
    );
}
function observe(p) {
    p.on("pageerror", (e) => failures.push(e.message));
    p.on("console", (m) => {
        if (m.type() === "error") failures.push(m.text());
    });
    p.on("response", (r) => {
        if (r.status() >= 400) failures.push(`${r.status()} ${r.url()}`);
    });
    p.on("requestfailed", (r) =>
        failures.push(`${r.failure()?.errorText} ${r.url()}`),
    );
}
async function fresh(p) {
    await button(p, "Restart journey").click();
    await button(p, "Restart from the forest").click();
    assert.equal(await number(p, "gold"), 300);
    assert.equal(await p.locator(".enemy").count(), 0);
    assert.equal(await p.locator('.pad[aria-label$="empty"]').count(), 10);
}
async function controls(browser) {
    const context = await browser.newContext({
        viewport: { width: 1440, height: 1000 },
        hasTouch: true,
    });
    const p = await context.newPage();
    observe(p);
    await ready(p);
    await shot(p, "main-menu");
    await start(p);
    await pad(p, 2).press("Enter");
    await button(p, /^Archer/).dblclick();
    assert.equal(await number(p, "gold"), 210);
    await button(p, /^Upgrade/).click();
    assert.equal(await number(p, "gold"), 100);
    await p.locator("#priority").selectOption("Strongest");
    await button(p, /^Upgrade/).click();
    await text(p, "[role=alert]", "Not enough gold");
    await pad(p, 5).click();
    await button(p, /^Catapult/).click();
    await text(p, "[role=alert]", "Not enough gold");
    await pad(p, 2).click();
    await button(p, /^Sell/).click();
    assert.equal(await number(p, "gold"), 200);
    await fresh(p);
    await pad(p, 2).click();
    await button(p, /^Archer/).click();
    await button(p, "Sound off").click();
    await button(p, "Sound on").waitFor();
    await button(p, /^Start wave 1 /).click();
    await p.locator(".enemy").first().waitFor();
    await button(p, "Pause").click();
    const frozen = await p.locator(".enemy").first().getAttribute("transform");
    await p.waitForTimeout(300);
    assert.equal(
        await p.locator(".enemy").first().getAttribute("transform"),
        frozen,
    );
    const saved = await p.evaluate(
        (k) => JSON.parse(localStorage.getItem(k)).State,
        runKey,
    );
    await p.reload();
    await button(p, "Continue journey →").waitFor({ timeout: 60000 });
    await button(p, "Continue journey →").click();
    await button(p, "Resume").waitFor();
    assert.equal(
        await p.locator(".enemy").first().getAttribute("transform"),
        frozen,
    );
    assert.equal(await number(p, "gold"), saved.Gold);
    await button(p, "Speed x1").click();
    await button(p, "Resume").click();
    await p.waitForTimeout(650);
    assert.notEqual(
        await p.locator(".enemy").first().getAttribute("transform"),
        frozen,
    );
    await p.evaluate(() => {
        Object.defineProperty(document, "hidden", {
            configurable: true,
            get: () => true,
        });
        document.dispatchEvent(new Event("visibilitychange"));
    });
    await button(p, "Resume").waitFor();
    await p.evaluate(() => {
        Object.defineProperty(document, "hidden", {
            configurable: true,
            get: () => false,
        });
        document.dispatchEvent(new Event("visibilitychange"));
    });
    await button(p, "Resume").waitFor();
    for (let i = 0; i < 3; i++) {
        await button(p, "Save & menu").click();
        await button(p, "Continue journey →").click();
        await button(p, "Resume").waitFor();
    }
    await fresh(p);
    await pad(p, 2).click();
    await button(p, /^Archer/).click();
    await shot(p, "battlefield");
    for (const [width, height] of [
        [390, 844],
        [844, 390],
    ]) {
        await p.setViewportSize({ width, height });
        assert.ok(
            await p.evaluate(
                () => document.documentElement.scrollWidth <= innerWidth,
            ),
        );
        if (width === 390) {
            for (let id = 1; id <= 10; id++) {
                await button(p, `Select pad ${id}`).tap();
            }
            await shot(p, "mobile");
        }
    }
    await p.setViewportSize({ width: 1440, height: 1000 });
    await button(p, /^Start wave 1 /).click();
    await p.locator(".enemy").first().waitFor();
    const before = await p.evaluate(
        (k) => JSON.parse(localStorage.getItem(k)).State.WaveTick,
        runKey,
    );
    await p.waitForTimeout(1100);
    await button(p, "Pause").click();
    const after = await p.evaluate(
        (k) => JSON.parse(localStorage.getItem(k)).State.WaveTick,
        runKey,
    );
    assert.ok(
        after - before >= 10 && after - before <= 40,
        `one clock after menu cycles: ${after - before}`,
    );
    report.push(
        "Controls, keyboard/touch, upgrades/sale/targeting, errors, pause/speed, live save/reload, hidden-tab event, menu cycles, laptop and mobile layouts passed",
    );
    console.log(report.at(-1));
    await context.close();
}
async function loss(browser, storage) {
    const context = await browser.newContext({
        viewport: { width: 1280, height: 900 },
    });
    if (storage === "corrupt")
        await context.addInitScript(
            ({ key, runKey }) => {
                localStorage.setItem(key, "{broken");
                localStorage.setItem(runKey, "{broken");
            },
            { key, runKey },
        );
    if (storage === "blocked")
        await context.addInitScript(() => {
            Storage.prototype.getItem = () => {
                throw new DOMException("Blocked", "SecurityError");
            };
            Storage.prototype.setItem = () => {
                throw new DOMException("Blocked", "SecurityError");
            };
        });
    const p = await context.newPage();
    observe(p);
    await ready(p);
    if (storage) assert.ok((await p.locator(".storage-warning").count()) > 0);
    await start(p);
    await button(p, "Speed x1").click();
    for (let wave = 1; wave <= 3; wave++) {
        await button(p, new RegExp(`^Start wave ${wave} `)).click();
        if (wave < 3)
            await button(p, new RegExp(`^Start wave ${wave + 1} `)).waitFor({
                timeout: 90000,
            });
        else
            await p
                .getByRole("heading", { name: "Defeat", exact: true })
                .waitFor({ timeout: 90000 });
    }
    await text(p, ".result-score", "300");
    await text(p, ".result", "300 points");
    if (!storage) {
        assert.equal(
            await p.evaluate((k) => localStorage.getItem(k), runKey),
            "ended",
        );
        await p.reload();
        await button(p, "New journey →").waitFor({ timeout: 60000 });
        assert.equal(await button(p, "Continue journey →").count(), 0);
        await text(p, ".record", "300 points");
        await start(p);
    } else await button(p, "Play again").click();
    assert.equal(await number(p, "gold"), 300);
    assert.equal(await p.locator('.pad[aria-label$="empty"]').count(), 10);
    await button(p, /^Start wave 1 /).waitFor();
    await button(p, "Speed x1").waitFor();
    report.push(
        `Defeat, record and fresh forest restart (${storage || "normal"} storage) passed`,
    );
    console.log(report.at(-1));
    await context.close();
}
async function victory(browser) {
    const context = await browser.newContext({
        viewport: { width: 1440, height: 1000 },
    });
    const p = await context.newPage();
    observe(p);
    await ready(p);
    await start(p);
    await button(p, "Speed x1").click();
    const order = [
        [2, "Archer"],
        [5, "Catapult"],
        [6, "Frost mage"],
        [1, "Archer"],
        [9, "Archer"],
        [8, "Catapult"],
        [4, "Archer"],
        [7, "Frost mage"],
        [10, "Archer"],
        [3, "Catapult"],
    ];
    for (let stage = 1; stage <= 3; stage++) {
        const waves = stage === 3 ? 15 : 12;
        for (let wave = 1; wave <= waves; wave++) {
            let allBuilt = true;
            for (const [id, kind] of order) {
                if (
                    !(await pad(p, id).getAttribute("aria-label")).endsWith(
                        "empty",
                    )
                )
                    continue;
                await pad(p, id).click();
                const card = button(p, new RegExp(`^${kind}`));
                const price = Number(
                    (await card.locator("b").innerText()).replace(/\D/g, ""),
                );
                if ((await number(p, "gold")) < price) {
                    allBuilt = false;
                    break;
                }
                await card.click();
            }
            if (allBuilt)
                for (const [id] of order) {
                    await pad(p, id).click();
                    while (await button(p, /^Upgrade/).count()) {
                        const upgrade = button(p, /^Upgrade/),
                            price = Number(
                                (await upgrade.innerText()).match(/\d+/)[0],
                            );
                        if ((await number(p, "gold")) < price) break;
                        await upgrade.click();
                    }
                }
            await button(p, new RegExp(`^Start wave ${wave} `)).click();
            if (wave === 5) {
                await p.waitForTimeout(2500);
                await shot(p, `chapter-${stage}`);
            }
            if (stage === 3 && wave === 15) {
                await p
                    .locator('.enemy[data-kind="Boss"]')
                    .waitFor({ timeout: 90000 });
                await shot(p, "boss");
            }
            if (wave < waves)
                await button(p, new RegExp(`^Start wave ${wave + 1} `)).waitFor(
                    { timeout: 120000 },
                );
            else if (stage < 3) {
                await button(p, "Travel to next region →").waitFor({
                    timeout: 120000,
                });
                const old = await p.evaluate(
                    (k) => JSON.parse(localStorage.getItem(k)).State,
                    runKey,
                );
                await p.reload();
                await button(p, "Continue journey →").waitFor({
                    timeout: 60000,
                });
                await button(p, "Continue journey →").click();
                await button(p, "Travel to next region →").click();
                const expected =
                    old.Gold +
                    old.Towers.reduce(
                        (sum, t) => sum + t.InvestedGold,
                        0,
                    ) +
                    75;
                assert.equal(await number(p, "gold"), expected);
                assert.equal(
                    await p.locator('.pad[aria-label$="empty"]').count(),
                    10,
                );
                const current = await p.evaluate(
                    (k) => JSON.parse(localStorage.getItem(k)).State,
                    runKey,
                );
                assert.equal(current.StageNumber, stage + 1);
                assert.equal(current.CoreHealth, old.CoreHealth);
            } else
                await p
                    .getByRole("heading", { name: "Victory", exact: true })
                    .waitFor({ timeout: 120000 });
            console.log(`Chapter ${stage}, wave ${wave}/${waves} complete`);
        }
    }
    const score = await number(p, "score");
    await shot(p, "victory");
    await p.waitForFunction(
        (k) => JSON.parse(localStorage.getItem(k))?.Won === true,
        key,
    );
    assert.equal(
        await p.evaluate((k) => localStorage.getItem(k), runKey),
        "ended",
    );
    await p.reload();
    await button(p, "New journey →").waitFor({ timeout: 60000 });
    assert.equal(await button(p, "Continue journey →").count(), 0);
    const stored = await p.evaluate(
        (k) => JSON.parse(localStorage.getItem(k)),
        key,
    );
    assert.equal(stored.Score, score);
    assert.equal(stored.WavesCleared, 39);
    assert.equal(stored.Won, true);
    report.push(
        `Complete legal 39-wave victory: ${score} points; both region saves/reloads, resource carryover and final record persisted`,
    );
    console.log(report.at(-1));
    await context.close();
}
(async () => {
    const server = process.env.GAME_URL
        ? null
        : await serve("artifacts/pages/wwwroot");
    let browser;
    try {
        browser = await chromium.launch({ headless: true });
        report.push(`Chromium ${browser.version()}`);
        if (process.env.TEST_MODE !== "victory") await controls(browser);
        if (process.env.TEST_MODE !== "victory") await mechanics(browser, url, observe, report);
        if (process.env.TEST_MODE !== "controls") await victory(browser);
        if (!process.env.TEST_MODE) {
            await loss(browser);
            await loss(browser, "corrupt");
            await loss(browser, "blocked");
        }
        await browser.close();
        if (process.env.BROWSERS !== "chromium" && !process.env.TEST_MODE) {
            browser = await firefox.launch({ headless: true });
            report.push(`Firefox ${browser.version()}`);
            await loss(browser);
            await browser.close();
        }
        browser = null;
        assert.deepEqual(failures, [], "Browser console/network failures");
        report.push(
            "No browser console errors, failed requests or HTTP errors",
        );
        fs.writeFileSync(
            "artifacts/browser-results.json",
            JSON.stringify(
                {
                    date: new Date().toISOString(),
                    url,
                    mode: process.env.TEST_MODE || "full",
                    passed: true,
                    report,
                },
                null,
                2,
            ),
        );
    } finally {
        if (browser) await browser.close();
        if (server) server.close();
    }
})().catch((error) => {
    fs.mkdirSync("artifacts", { recursive: true });
    fs.writeFileSync(
        "artifacts/browser-results.json",
        JSON.stringify(
            { passed: false, report, failures, error: error.stack },
            null,
            2,
        ),
    );
    console.error(error);
    process.exitCode = 1;
});
