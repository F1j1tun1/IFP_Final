const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const { chromium, firefox } = require("playwright");
const { serve } = require("./server.cjs");
const url = process.env.GAME_URL || "http://127.0.0.1:5080/IFP_Final/";
const key = "chrono-bastion.high-score.v1";
const screenshots = path.resolve("artifacts/screenshots");
const report = [];
const failures = [];
fs.mkdirSync(screenshots, { recursive: true });
const button = (page, name) =>
    page.getByRole("button", { name, exact: typeof name === "string" });
const pad = (page, id) => page.locator(`.pad[aria-label^="Pad ${id}:"]`);
const number = async (page, id) =>
    Number((await page.getByTestId(id).innerText()).replace(/[^0-9]/g, ""));
async function ready(page) {
    await page.goto(url);
    await button(page, "Deploy defences →").waitFor({ timeout: 60000 });
}
async function start(page) {
    await button(page, "Deploy defences →").click();
}
async function waitText(page, selector, text, timeout = 10000) {
    await page.waitForFunction(
        ({ selector, text }) =>
            document.querySelector(selector)?.textContent.includes(text),
        { selector, text },
        { timeout },
    );
}
async function fresh(page) {
    await button(page, "Restart run").click();
    await button(page, "Restart from wave one").click();
    assert.equal(await number(page, "gold"), 300);
    assert.equal(await page.locator(".enemy").count(), 0);
    assert.equal(await page.locator('.pad[aria-label$="empty"]').count(), 10);
}
function observe(page) {
    page.on("pageerror", (error) => failures.push(error.message));
    page.on("console", (message) => {
        if (message.type() === "error") failures.push(message.text());
    });
    page.on("response", (response) => {
        if (response.status() >= 400)
            failures.push(`${response.status()} ${response.url()}`);
    });
    page.on("requestfailed", (request) =>
        failures.push(`${request.failure()?.errorText} ${request.url()}`),
    );
}
async function controls(browser) {
    const context = await browser.newContext({
        viewport: { width: 1440, height: 1000 },
        hasTouch: true,
    });
    const page = await context.newPage();
    observe(page);
    await ready(page);
    await page.screenshot({
        path: path.join(screenshots, "main-menu.png"),
        fullPage: true,
    });
    await start(page);
    await pad(page, 2).click();
    await button(page, /^Pulse/).dblclick();
    assert.equal(await number(page, "gold"), 210);
    await button(page, /^Upgrade/).click();
    assert.equal(await number(page, "gold"), 135);
    await page.locator("#priority").selectOption("Strongest");
    assert.equal(await page.locator("#priority").inputValue(), "Strongest");
    await button(page, /^Upgrade/).click();
    await waitText(page, ".max-level", "Max level");
    await pad(page, 5).click();
    await button(page, /^Mortar/).click();
    await waitText(page, "[role=alert]", "Not enough gold");
    await pad(page, 2).click();
    await button(page, /^Sell/).click();
    assert.equal(await number(page, "gold"), 214);
    await fresh(page);
    await pad(page, 2).click();
    await button(page, /^Pulse/).click();
    await button(page, /^Start wave 1/).click();
    await page.locator(".enemy").first().waitFor();
    await button(page, "Pause").click();
    const frozen = await page
        .locator(".enemy")
        .first()
        .getAttribute("transform");
    await page.waitForTimeout(300);
    assert.equal(
        await page.locator(".enemy").first().getAttribute("transform"),
        frozen,
    );
    await button(page, "Speed x1").click();
    await button(page, "Speed x2").waitFor();
    await button(page, "Resume").click();
    await page.waitForTimeout(700);
    assert.notEqual(
        await page.locator(".enemy").first().getAttribute("transform"),
        frozen,
    );
    // Dispatch the real visibility event with the hidden browser capability emulated.
    await page.evaluate(() => {
        Object.defineProperty(document, "hidden", {
            configurable: true,
            get: () => true,
        });
        document.dispatchEvent(new Event("visibilitychange"));
    });
    await button(page, "Resume").waitFor();
    await page.evaluate(() => {
        delete document.hidden;
        document.dispatchEvent(new Event("visibilitychange"));
    });
    await page.waitForTimeout(200);
    await button(page, "Resume").waitFor();
    await button(page, "Resume").click();
    await button(page, "Field manual").click();
    await button(page, "Close manual").click();
    await button(page, "Resume").waitFor();
    await page.screenshot({
        path: path.join(screenshots, "battlefield.png"),
        fullPage: true,
    });
    for (let i = 0; i < 3; i++) {
        await button(page, "Main menu").click();
        await button(page, "Discard and return to menu").click();
        await start(page);
    }
    await button(page, /^Start wave 1/).click();
    await page.waitForTimeout(1000);
    await button(page, "Pause").click();
    const transform = await page
        .locator(".enemy")
        .first()
        .getAttribute("transform");
    const distance = Number(transform.match(/translate\(([\d.]+)/)[1]);
    assert(
        distance >= 30 && distance < 120,
        `Clock after transitions: ${distance}`,
    );
    assert.equal(
        await page.evaluate((k) => localStorage.getItem(k), key),
        null,
    );
    await fresh(page);
    await page.setViewportSize({ width: 390, height: 844 });
    for (let id = 1; id <= 10; id++) {
        await pad(page, id).tap();
        await waitText(page, ".tower-panel h2", `pad ${id}`);
    }
    await button(page, /^Pulse/).click();
    assert(
        await page.evaluate(
            () => document.documentElement.scrollWidth <= innerWidth,
        ),
    );
    await page.screenshot({
        path: path.join(screenshots, "mobile.png"),
        fullPage: true,
    });
    await page.setViewportSize({ width: 844, height: 390 });
    assert(
        await page.evaluate(
            () => document.documentElement.scrollWidth <= innerWidth,
        ),
    );
    report.push(
        "Controls, errors, pause/resume, speed, emulated hidden-tab event, repeated menu transitions, 390x844 and 844x390 layouts passed",
    );
    await context.close();
}
async function loss(browser, storage) {
    const context = await browser.newContext({
        viewport: { width: 1280, height: 900 },
    });
    if (storage === "corrupt")
        await context.addInitScript(
            (k) => localStorage.setItem(k, "{broken"),
            key,
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
    const page = await context.newPage();
    observe(page);
    await ready(page);
    if (storage) await page.locator(".storage-warning").waitFor();
    await start(page);
    await button(page, "Speed x1").click();
    for (let wave = 1; wave <= 3; wave++) {
        await button(page, new RegExp(`^Start wave ${wave}`)).click();
        if (wave < 3)
            await button(page, new RegExp(`^Start wave ${wave + 1}`)).waitFor({
                timeout: 60000,
            });
        else
            await page
                .getByRole("heading", { name: "Defeat", exact: true })
                .waitFor({ timeout: 60000 });
    }
    await waitText(page, ".result-score", "300");
    await waitText(page, ".result", "300 points");
    if (storage === "blocked")
        await waitText(page, ".storage-warning", "could not be saved");
    if (!storage) {
        await page.reload();
        await button(page, "Deploy defences →").waitFor();
        await waitText(page, ".record", "300 points");
        await start(page);
    } else await button(page, "Play again").click();
    assert.equal(await number(page, "gold"), 300);
    assert.equal(await page.locator('.pad[aria-label$="empty"]').count(), 10);
    await button(page, /^Start wave 1/).waitFor();
    await button(page, "Speed x1").waitFor();
    report.push(
        `Complete defeat, record and fresh restart (${storage || "normal"} storage) passed`,
    );
    await context.close();
}
async function victory(browser) {
    const context = await browser.newContext({
        viewport: { width: 1440, height: 1000 },
        hasTouch: true,
    });
    const page = await context.newPage();
    observe(page);
    await ready(page);
    await start(page);
    await button(page, "Speed x1").click();
    const order = [
        [2, "Pulse"],
        [5, "Mortar"],
        [6, "Frost"],
        [1, "Pulse"],
        [9, "Pulse"],
        [8, "Mortar"],
        [4, "Pulse"],
        [7, "Frost"],
        [10, "Pulse"],
        [3, "Mortar"],
    ];
    for (let wave = 1; wave <= 10; wave++) {
        let allBuilt = true;
        for (const [id, kind] of order) {
            if (
                !(await pad(page, id).getAttribute("aria-label")).endsWith(
                    "empty",
                )
            )
                continue;
            await pad(page, id).click();
            const card = button(page, new RegExp(`^${kind}`));
            const price = Number(
                await card
                    .locator("b")
                    .innerText()
                    .then((t) => t.replace(/\D/g, "")),
            );
            if ((await number(page, "gold")) < price) {
                allBuilt = false;
                break;
            }
            await card.click();
        }
        if (allBuilt)
            for (const [id] of order) {
                await pad(page, id).click();
                while (await button(page, /^Upgrade/).count()) {
                    const upgrade = button(page, /^Upgrade/);
                    const price = Number(
                        (await upgrade.innerText()).match(/\d+/)[0],
                    );
                    if ((await number(page, "gold")) < price) break;
                    await upgrade.click();
                }
            }
        await button(page, new RegExp(`^Start wave ${wave}`)).click();
        if (wave === 5) {
            await page.waitForTimeout(2000);
            await page.screenshot({
                path: path.join(screenshots, "combat.png"),
                fullPage: true,
            });
        }
        if (wave < 10)
            await button(page, new RegExp(`^Start wave ${wave + 1}`)).waitFor({
                timeout: 90000,
            });
        else
            await page
                .getByRole("heading", { name: "Victory", exact: true })
                .waitFor({ timeout: 90000 });
        console.log(`Browser wave ${wave} complete`);
    }
    assert.equal(await number(page, "score"), 12090);
    await waitText(page, ".result", "151");
    await page.screenshot({
        path: path.join(screenshots, "victory.png"),
        fullPage: true,
    });
    await page.reload();
    await button(page, "Deploy defences →").waitFor();
    await waitText(page, ".record", "12");
    const stored = await page.evaluate(
        (k) => JSON.parse(localStorage.getItem(k)),
        key,
    );
    assert.equal(stored.Score, 12090);
    assert.equal(stored.Won, true);
    report.push(
        "Complete legal browser victory: 12090 points, 151 kills; record persisted after reload",
    );
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
        await controls(browser);
        await victory(browser);
        await loss(browser);
        await loss(browser, "corrupt");
        await loss(browser, "blocked");
        await browser.close();
        if (process.env.BROWSERS !== "chromium") {
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
        console.log(report.join("\n"));
        fs.mkdirSync("artifacts", { recursive: true });
        fs.writeFileSync(
            "artifacts/browser-results.json",
            JSON.stringify(
                { date: new Date().toISOString(), url, passed: true, report },
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
            {
                date: new Date().toISOString(),
                url,
                passed: false,
                report,
                failures,
                error: error.message,
            },
            null,
            2,
        ),
    );
    console.error(error);
    process.exitCode = 1;
});
