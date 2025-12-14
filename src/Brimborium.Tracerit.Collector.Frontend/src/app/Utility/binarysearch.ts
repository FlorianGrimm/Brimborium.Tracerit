import { ZonedDateTime } from "@js-joda/core";

export function binarySearchById<T extends { id: number }>(
    id: number,
    array: T[]): T | null {
    if (array.length === 0) { return null; }
    let left = 0;
    let right = array.length - 1;
    {
        const firstId = array[left].id;
        const lastId = array[right].id;
        const linear = Math.floor(array.length * ((id - firstId) / (lastId - firstId)));
        if ((0 <= linear) && (linear <= right)) {
            if (array[linear].id === id) {
                return array[linear];
            } else if (array[linear].id < id) {
                left = linear - 1;
            } else {
                right = linear + 1;
            }
        }
    }
    {
        while (left <= right) {
            const mid = Math.floor((right - left) / 2) + left;
            if (array[mid].id === id) {
                return array[mid];
            } else if (array[mid].id < id) {
                left = mid + 1;
            } else {
                right = mid - 1;
            }
        }
    }
    return null;
}

export function binarySearchByTS<T extends { ts: ZonedDateTime | null }>(
    ts: ZonedDateTime,
    array: T[]): T | null {
    let left = 0;
    let right = array.length - 1;
    while (left <= right) {
        let mid = Math.floor((right - left) / 2) + left;
        if (array[mid].ts == null) {
            while (array[mid].ts == null) {
                mid++;
                if (array.length <= mid) { return null; }
            }
        }
        const cmp = array[mid].ts!.compareTo(ts);
        if (cmp === 0) {
            return array[mid];
        } else if (cmp < 0) {
            left = mid + 1;
        } else {
            right = mid - 1;
        }
    }
    return null;
}
