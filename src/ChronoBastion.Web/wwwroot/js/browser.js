export function watchVisibility(reference) {
    const changed = () => {
        if (document.hidden) flushRun();
        reference.invokeMethodAsync("OnVisibilityChanged", document.hidden).catch(() => {});
    };
    document.addEventListener("visibilitychange", changed);
    window.addEventListener("pagehide", flushRun);
    changed();
    return {
        dispose: () => {
            document.removeEventListener("visibilitychange", changed);
            window.removeEventListener("pagehide", flushRun);
        },
    };
}

// Keep keyboard focus inside the currently open dialog and restore its trigger.
export function watchDialogs() {
    let current = null;
    let previous = null;
    const observer = new MutationObserver(() => {
        const next = document.querySelector("[data-dialog]");
        if (next === current) return;
        if (next) {
            if (!current) previous = document.activeElement;
            current = next;
            next.focus();
        } else {
            current = null;
            if (previous?.isConnected) previous.focus();
            previous = null;
        }
    });
    const keydown = (event) => {
        if (!current || event.key !== "Tab") return;
        const items = [
            ...current.querySelectorAll(
                'button:not(:disabled), select, a[href], [tabindex="0"]',
            ),
        ];
        if (!items.length) {
            event.preventDefault();
            return;
        }
        const first = items[0],
            last = items.at(-1);
        if (
            event.shiftKey &&
            (document.activeElement === first ||
                document.activeElement === current)
        ) {
            event.preventDefault();
            last.focus();
        } else if (
            !event.shiftKey &&
            (document.activeElement === last ||
                document.activeElement === current)
        ) {
            event.preventDefault();
            first.focus();
        }
    };
    observer.observe(document.body, { childList: true, subtree: true });
    document.addEventListener("keydown", keydown);
    return {
        dispose: () => {
            observer.disconnect();
            document.removeEventListener("keydown", keydown);
        },
    };
}

const runKey = "thornwatch.run.v2";
let latestRun = null;
let savedRun = null;
let storageStatus = "unavailable";
let releaseLock = null;
let storageListener = null;
let saveReference = null;

function conflict() {
    if (storageStatus === "conflict") return;
    storageStatus = "conflict";
    saveReference?.invokeMethodAsync("OnStorageConflict").catch(() => {});
}

export async function claimRun(reference) {
    saveReference = reference;
    if (!navigator.locks) return "unavailable";
    return new Promise(resolve => {
        navigator.locks.request(runKey, { ifAvailable: true }, async lock => {
            if (!lock) {
                storageStatus = "conflict";
                resolve(storageStatus);
                return;
            }
            try {
                savedRun = localStorage.getItem(runKey);
                storageStatus = "ready";
            } catch {
                resolve("unavailable");
                return;
            }
            storageListener = event => {
                if (event.key !== runKey && event.key !== null) return;
                try {
                    if (localStorage.getItem(runKey) !== savedRun) conflict();
                } catch {
                    storageStatus = "unavailable";
                }
            };
            window.addEventListener("storage", storageListener);
            const held = new Promise(release => { releaseLock = release; });
            resolve(storageStatus);
            await held;
        }).catch(() => resolve("unavailable"));
    });
}

export function releaseRun() {
    if (storageListener) window.removeEventListener("storage", storageListener);
    storageListener = null;
    storageStatus = "unavailable";
    releaseLock?.();
    releaseLock = null;
    saveReference = null;
}

export function reload() { window.location.reload(); }

export function cacheRun(json) {
    latestRun = json;
}
export function flushRun() {
    if (storageStatus === "conflict" || storageStatus === "unavailable") return storageStatus;
    try {
        if (localStorage.getItem(runKey) !== savedRun) {
            conflict();
            return "conflict";
        }
        if (latestRun === null) return "idle";
        if (latestRun !== savedRun) {
            localStorage.setItem(runKey, latestRun);
            savedRun = latestRun;
        }
        return "saved";
    } catch {
        return "unavailable";
    }
}
let audio = null;
let enabled = false;
let lastCue = 0;
export async function setSound(value) {
    if (value && !audio) audio = new AudioContext();
    if (value) await audio.resume();
    enabled = value;
    if (value) playCue("build");
    return enabled;
}
export function playCue(kind) {
    if (!enabled || !audio || document.hidden) return;
    const now = audio.currentTime;
    if (now - lastCue < 0.1) return;
    lastCue = now;
    const voices = {
        Pulse: [860, 290, 0.07, "triangle"],
        Mortar: [130, 38, 0.18, "sine"],
        Frost: [1200, 1800, 0.16, "sine"],
        build: [400, 800, 0.16, "triangle"],
        wave: [220, 330, 0.32, "triangle"],
        boss: [160, 55, 0.4, "sawtooth"],
        win: [440, 880, 0.6, "triangle"],
        loss: [220, 60, 0.6, "sine"],
    };
    const [from, to, duration, type] = voices[kind] || voices.build;
    const oscillator = audio.createOscillator();
    const gain = audio.createGain();
    oscillator.type = type;
    oscillator.frequency.setValueAtTime(from, now);
    oscillator.frequency.exponentialRampToValueAtTime(to, now + duration);
    gain.gain.setValueAtTime(0.0001, now);
    gain.gain.exponentialRampToValueAtTime(0.045, now + 0.008);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + duration);
    oscillator.connect(gain).connect(audio.destination);
    oscillator.onended = () => {
        oscillator.disconnect();
        gain.disconnect();
    };
    oscillator.start(now);
    oscillator.stop(now + duration);
}
export async function disposeAudio() {
    enabled = false;
    if (audio) await audio.close();
    audio = null;
}
