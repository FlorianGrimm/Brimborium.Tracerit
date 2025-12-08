import { getEffectiveRange } from "./time-range";
import { Duration, ZonedDateTime, ZoneId } from '@js-joda/core';

describe('getEffectiveRange', () => {
    it('getEffectiveRange empty list', () => {
        const act = getEffectiveRange([]);
        expect(true).toBeTruthy();
    });

    it('getEffectiveRange one', () => {
        const input = [
            {
                start: ZonedDateTime.of(2000, 1, 1, 0, 0, 0, 0, ZoneId.UTC),
                finish: ZonedDateTime.of(2000, 1, 31, 0, 0, 0, 0, ZoneId.UTC),
            }
        ];

        const act = getEffectiveRange(input);

        expect((act.start).toString()).toBe('2000-01-01T00:00Z');
        expect((act.finish).toString()).toBe('2000-01-31T00:00Z');
    });

    it('getEffectiveRange two', () => {
        const input = [
            {
                start: ZonedDateTime.of(2000, 1, 1, 0, 0, 0, 0, ZoneId.UTC),
                finish: ZonedDateTime.of(2000, 1, 31, 0, 0, 0, 0, ZoneId.UTC),
            },

            {
                start: ZonedDateTime.of(2000, 1, 3, 0, 0, 0, 0, ZoneId.UTC),
                finish: ZonedDateTime.of(2000, 1, 29, 0, 0, 0, 0, ZoneId.UTC),
            }

        ];
        const act = getEffectiveRange(input);

        expect((act.start).toString()).toBe('2000-01-03T00:00Z');
        expect((act.finish).toString()).toBe('2000-01-29T00:00Z');
    });

});
