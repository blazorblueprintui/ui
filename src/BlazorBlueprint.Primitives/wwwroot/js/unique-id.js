/**
 * Random IDs for the cleanup registries, on any page.
 *
 * `crypto.randomUUID()` only exists in a secure context — https, or localhost. A site served over
 * plain HTTP on any other host has no such function, and calling it threw inside overlay setup:
 * the overlay was already on screen, its dismissal listeners were never wired, and nothing could
 * close it. `crypto.getRandomValues()` is available everywhere, so it builds the same v4 UUID when
 * `randomUUID` is missing.
 *
 * These IDs are only registry keys. Nothing here is security-sensitive.
 *
 * @returns {string} A version 4 UUID.
 */
export function createId() {
    if (typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID();
    }

    const bytes = crypto.getRandomValues(new Uint8Array(16));
    bytes[6] = (bytes[6] & 0x0f) | 0x40; // version 4
    bytes[8] = (bytes[8] & 0x3f) | 0x80; // RFC 4122 variant

    const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('');
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}
