import { liveAPI } from "@/services/config"
import { useEffect, useState } from "react"
import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer,
    LineChart,
    Line,
} from "recharts"

type Statistic = {
    id: string
    liveTitle: string
    uniqueViewers: number
    totalDuration: string // reçu en ISO => converti plus tard
    period: string
}

const LiveStatistics = () => {
    const [stats, setStats] = useState<Statistic[]>([])
    const [range, setRange] = useState("7d")
    const [selectedData, setSelectedData] = useState<string>("viewer")
    const DEFAULT_COLOR: string = "#E0295D"
    const [navElements, setElements] = useState<
        { id: number; content: string }[]
    >([
        { id: 1, content: "Viewer" },
        { id: 2, content: "Durée" },
    ])
    const [currentDataView, setCurrentDataView] = useState<number>(
        navElements[0].id
    )
    const [navElementRange, setNNavElementRange] = useState<
        { id: number; content: string; value: string }[]
    >([
        { id: 1, content: "Aujourd'hui", value: "day" },
        { id: 2, content: "Hier", value: "yesterday" },
        { id: 3, content: "7 derniers jours", value: "7d" },
        { id: 4, content: "Mois dernier", value: "month" },
        { id: 5, content: "3 derniers mois", value: "3m" },
        { id: 6, content: "Année dernière", value: "year" },
    ])
    const [currentPeriodView, setCurrentPeriodView] = useState<number>(() => {
        const found = [
            { id: 1, content: "Aujourd'hui", value: "day" },
            { id: 2, content: "Hier", value: "yesterday" },
            { id: 3, content: "7 derniers jours", value: "7d" },
            { id: 4, content: "Mois dernier", value: "month" },
            { id: 5, content: "3 derniers mois", value: "3m" },
            { id: 6, content: "Année dernière", value: "year" },
        ].find((x) => x.value === "7d")
        return found ? found.id : 1
    })

    const handleRangeTabClick = (itemId: number) => {
        const item = navElementRange.find((x) => x.id === itemId)
        if (!item) return
        setCurrentPeriodView(item.id)
        setRange(item.value) // déclenchera fetchStats via useEffect([range])
    }

    async function fetchStats() {
        try {
            const response = await fetch(
                `${liveAPI}/statistic?range=${range}`,
                {
                    method: "GET",
                    headers: { "Content-Type": "application/json" },
                    credentials: "include",
                }
            )
            if (!response.ok) {
                throw new Error("Network response was not ok")
            }
            const data = await response.json()
            setStats(data)
        } catch (error) {
            console.error("Error fetching stats:", error)
        }
    }

    useEffect(() => {
        fetchStats()
    }, [range])

    const formatDuration = (duration: string) => {
        // Cas "hh:mm:ss" (ex: "01:23:45")
        if (/^\d{2}:\d{2}:\d{2}$/.test(duration)) {
            const [h, m, s] = duration.split(":").map(Number)
            return h * 60 + m + Math.floor(s / 60)
        }

        // Cas TimeSpan avec fractions "hh:mm:ss.ffffff" (ex: "00:01:17.8894620")
        if (/^\d{2}:\d{2}:\d{2}\.\d+$/.test(duration)) {
            const [hms, fraction] = duration.split(".")
            const [h, m, s] = hms.split(":").map(Number)
            return h * 60 + m + Math.floor(s / 60)
        }

        console.warn("Unknown duration format:", duration)
        return 0
    }

    return (
        <div className="space-y-8 p-8 backdrop-blur-md c-box-shadow-wrapper c-base-wrapper-component border-2 border-[var(--border-main)] rounded-[12px] w-full c-box-shadow-wrapper relative overflow-hidden">
            <div className="mb-4">
                <label className="mr-2 c-h3-title text-[var(--text-primary)]">
                    Données à visionner
                </label>
                <div className="c-option-theme__wrapper my-4">
                    {navElements.map((item, index: number) => (
                        <div
                            onClick={() => setCurrentDataView(item.id)}
                            key={index}
                            className={
                                currentDataView != item.id
                                    ? "c-single-option"
                                    : "c-single-option__active"
                            }
                        >
                            <p className="text-[14px] font-medium">
                                {item.content}
                            </p>
                        </div>
                    ))}
                </div>
            </div>

            <div className="mb-4">
                <label className="mr-2 c-h3-title text-[var(--text-primary)]">
                    Période
                </label>

                {/* Desktop pills */}
                <div className="hidden md:flex">
                    <div className="c-option-theme__wrapper my-4 ">
                        {navElementRange.map((item) => (
                            <div
                                key={item.id}
                                onClick={() => handleRangeTabClick(item.id)}
                                className={
                                    currentPeriodView !== item.id
                                        ? "c-single-option"
                                        : "c-single-option__active"
                                }
                                role="button"
                                tabIndex={0}
                                onKeyDown={(e) =>
                                    (e.key === "Enter" || e.key === " ") &&
                                    handleRangeTabClick(item.id)
                                }
                                aria-pressed={currentPeriodView === item.id}
                            >
                                <p className="text-[14px] font-medium">
                                    {item.content}
                                </p>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Mobile select */}
                <div className="md:hidden mt-2">
                    <select
                        value={range}
                        onChange={(e) => setRange(e.target.value)}
                        className="c-simple-input c-simple-input--rounded w-full"
                    >
                        {navElementRange.map((opt) => (
                            <option key={opt.id} value={opt.value}>
                                {opt.content}
                            </option>
                        ))}
                    </select>
                </div>
            </div>

            {currentDataView === 1 ? (
                <div className="bg-[var(--background-secondary)] shadow rounded-2xl p-4 text-[var(--text-primary)]">
                    <h3 className="text-lg font-semibold mb-2 ">
                        Nombre de viewers uniques
                    </h3>
                    <ResponsiveContainer width="100%" height={300} className="">
                        <LineChart data={stats}>
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="period" />
                            <YAxis />
                            <Tooltip contentStyle={{ color: "black" }} />
                            <Legend />
                            <Line
                                type="monotone"
                                dataKey="uniqueViewers"
                                stroke={DEFAULT_COLOR}
                                name="Viewers"
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </div>
            ) : currentDataView === 2 ? (
                <div className="bg-[var(--background-secondary)] shadow rounded-2xl p-4 text-[var(--text-primary)]">
                    <h3 className="text-lg font-semibold mb-2 ">
                        Durée totale du stream (en minutes)
                    </h3>
                    <ResponsiveContainer width="100%" height={300}>
                        <BarChart
                            data={stats.map((s) => ({
                                ...s,
                                durationMinutes: formatDuration(
                                    s.totalDuration
                                ),
                            }))}
                        >
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="period" />
                            <YAxis />
                            <Tooltip contentStyle={{ color: "black" }} />
                            <Legend />
                            <Bar
                                dataKey="durationMinutes"
                                fill={DEFAULT_COLOR}
                                name="Durée (min)"
                            />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            ) : null}
        </div>
    )
}

export default LiveStatistics

