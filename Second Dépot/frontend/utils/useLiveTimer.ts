import { useEffect, useMemo, useState } from "react";

function pad(n: number) {
  return String(n).padStart(2, "0");
}

function formatHMS(totalSeconds: number) {
  const sign = totalSeconds < 0 ? "-" : "";
  const s = Math.abs(totalSeconds);
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  const sec = Math.floor(s % 60);
  return h > 0 ? `${sign}${h}:${pad(m)}:${pad(sec)}` : `${sign}${pad(m)}:${pad(sec)}`;
}

type TimerState = {
  isBeforeStart: boolean;
  isLive: boolean;
  label: string;        // Texte prêt à afficher
  seconds: number;      // Différence en secondes (utile si tu veux autre chose)
};

export function useLiveTimer(startISO?: string, endISO?: string): TimerState {
  const [now, setNow] = useState<number>(() => Date.now());

  // Update toute les 1s
  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, []);

  const { isBeforeStart, isLive, label, seconds } = useMemo(() => {
    if (!startISO) {
      return { isBeforeStart: false, isLive: false, label: "", seconds: 0 };
    }
    const start = Date.parse(startISO); // ISO -> ms UTC (correct pour fuseaux)
    const end = endISO ? Date.parse(endISO) : undefined;

    if (isNaN(start)) {
      return { isBeforeStart: false, isLive: false, label: "", seconds: 0 };
    }

    if (now < start) {
      // Compte à rebours avant le début
      const secsLeft = Math.max(0, Math.floor((start - now) / 1000));
      return {
        isBeforeStart: true,
        isLive: false,
        label: `Démarre dans ${formatHMS(secsLeft)}`,
        seconds: -secsLeft,
      };
    }

    // Si on a un endTime et qu'il est passé
    if (end !== undefined && now >= end) {
      const total = Math.floor((end - start) / 1000);
      return {
        isBeforeStart: false,
        isLive: false,
        label: `Terminé • ${formatHMS(total)}`,
        seconds: total,
      };
    }

    // Sinon en direct
    const elapsed = Math.floor((now - start) / 1000) - 2 * 3600;
    return {
      isBeforeStart: false,
      isLive: true,
      label: `${formatHMS(elapsed)}`,
      seconds: elapsed,
    };
  }, [startISO, endISO, now]);

  return { isBeforeStart, isLive, label, seconds };
}
