// ProgrammedLive.tsx
import { liveAPI } from "@/services/config"
import { useEffect, useMemo, useRef, useState } from "react"
import ProgrammedLiveModify from "./ProgrammedLiveModify"
import { useNavigate } from "react-router-dom"
import { Live } from "../types/LiveTypes"
import { ROOT_APP_URL } from "@/services/navigation"
import { set } from "date-fns"
import Toast from "@/components/Base/Toast/Toast"

const ProgrammedLive = ({ refreshKey }: { refreshKey: any }) => {
    const [lives, setLives] = useState<Live[]>([])
    const [visibleFields, setVisibleFields] = useState<
        Record<string, { url: boolean; key: boolean }>
    >({})
    const [copied, setCopied] = useState<Record<string, boolean>>({})
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
    const [bulkDeleting, setBulkDeleting] = useState(false)
    const [refreshKeyModify, setRefreshKeyModify] = useState(0)
    const [modifyOpen, setModifyOpen] = useState(false)
    const [editingId, setEditingId] = useState<string | null>(null)
    const [editingTitle, setEditingTitle] = useState<string>("")
    const [editingIsPublic, setEditingIsPublic] = useState<boolean>(true)
    const [editingThumbnailUrl, setEditingThumbnailUrl] = useState<
        string | null
    >(null)
    const [editingInvitedUserId, setEditingInvitedUserId] = useState<
        number | null
    >(null)
    const [toast, setToast] = useState<{
        type: "success" | "failed" | "warn" | "info"
        message: string
    } | null>(null)

    const navigate = useNavigate()

    const selectAllRef = useRef<HTMLInputElement | null>(null)

    function handleLiveModified() {
        setRefreshKeyModify((prev) => prev + 1)
    }

    function handleModify(id: string) {
        const live = lives.find((l) => l.apiVideoLiveStreamId === id)
        if (!live) return
        setEditingId(id)
        setEditingTitle(live.title)
        setEditingIsPublic(live.isPublic ?? true)
        setEditingThumbnailUrl(live.thumbnailUrl ?? null)
        setEditingInvitedUserId(live.invitedUserId ?? null)
        // setEditingStartTime(live.startTime)
        // setEditingEndTime(live.endTime)
        setModifyOpen(true)
    }

    function toggleVisibility(id: string, field: "url" | "key") {
        setVisibleFields((prev) => ({
            ...prev,
            [id]: {
                url: field === "url" ? !prev[id]?.url : prev[id]?.url ?? false,
                key: field === "key" ? !prev[id]?.key : prev[id]?.key ?? false,
            },
        }))
    }

    async function handleCopy(id: string, text: string) {
        try {
            await navigator.clipboard.writeText(text)
            setCopied((prev) => ({ ...prev, [id]: true }))
            setTimeout(() => {
                setCopied((prev) => ({ ...prev, [id]: false }))
            }, 1200)
        } catch (e) {
            console.error("Échec de la copie", e)
        }
    }

    async function fetchLives() {
        try {
            const response = await fetch(`${liveAPI}/streamer`, {
                method: "GET",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
            })
            if (!response.ok) {
                console.error("Failed to fetch lives:", response.statusText)
                return
            }
            const data: Live[] = await response.json()
            setLives(data)

            // Nettoyer la sélection d'IDs qui n'existent plus
            setSelectedIds((prev) => {
                const next = new Set<string>()
                const ids = new Set(data.map((l) => l.apiVideoLiveStreamId))
                prev.forEach((id) => {
                    if (ids.has(id)) next.add(id)
                })
                return next
            })
        } catch (error) {
            console.error("Failed to fetch lives:", error)
        }
    }

    async function handleClickDeleteLive(id: string) {
        const confirmed = window.confirm(
            "Êtes-vous sûr de vouloir supprimer ce live ? Cette action est irréversible."
        )
        if (!confirmed) return

        try {
            const resp = await fetch(`${liveAPI}/${id}`, {
                method: "DELETE",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            })

            if (!resp.ok) {
                console.error(
                    "Échec de la suppression du live:",
                    resp.statusText
                )
                setToast({
                    type: "failed",
                    message: "Echec de la suppression du live",
                })
                return
            }

            // Optimiste
            setLives((prev) =>
                prev.filter((l) => l.apiVideoLiveStreamId !== id)
            )
            setSelectedIds((prev) => {
                const next = new Set(prev)
                next.delete(id)
                return next
            })
        } catch (e) {
            console.error("Erreur lors de la suppression du live:", e)
            setToast({
                type: "failed",
                message: "Echec de la suppression du live",
            })
        }
    }

    // Sélection individuelle
    function toggleSelect(id: string) {
        setSelectedIds((prev) => {
            const next = new Set(prev)
            if (next.has(id)) next.delete(id)
            else next.add(id)
            return next
        })
    }

    // Sélection globale
    const allIds = useMemo(
        () => lives.map((l) => l.apiVideoLiveStreamId),
        [lives]
    )
    const allSelected =
        selectedIds.size > 0 && selectedIds.size === lives.length
    const isIndeterminate =
        selectedIds.size > 0 && selectedIds.size < lives.length

    useEffect(() => {
        if (selectAllRef.current) {
            selectAllRef.current.indeterminate = isIndeterminate
        }
    }, [isIndeterminate, selectedIds.size, lives.length])

    function toggleSelectAll() {
        setSelectedIds((prev) => {
            if (prev.size === lives.length) return new Set() // tout désélectionner
            return new Set(allIds) // tout sélectionner
        })
    }

    // Suppression en lot
    async function handleBulkDelete() {
        if (selectedIds.size === 0) return
        const confirmed = window.confirm(
            `Supprimer ${selectedIds.size} live(s) ? Cette action est irréversible.`
        )
        if (!confirmed) return

        setBulkDeleting(true)
        try {
            const idsToDelete = Array.from(selectedIds)
            const results = await Promise.allSettled(
                idsToDelete.map((id) =>
                    fetch(`${liveAPI}/${id}`, {
                        method: "DELETE",
                        credentials: "include",
                        headers: { "Content-Type": "application/json" },
                    })
                )
            )

            const succeededIds: string[] = []
            const failed: string[] = []
            results.forEach((res, idx) => {
                const id = idsToDelete[idx]
                if (res.status === "fulfilled" && (res.value as Response).ok) {
                    succeededIds.push(id)
                } else {
                    failed.push(id)
                }
            })

            if (succeededIds.length > 0) {
                setLives((prev) =>
                    prev.filter(
                        (l) => !succeededIds.includes(l.apiVideoLiveStreamId)
                    )
                )
            }
            setSelectedIds((prev) => {
                const next = new Set(prev)
                succeededIds.forEach((id) => next.delete(id))
                return next
            })

            if (failed.length > 0) {
                setToast({
                    type: "failed",
                    message: `Impossible de supprimer ${failed.length} live(s). Réessayez.`,
                })
            }
        } catch (e) {
            console.error("Erreur lors de la suppression en lot:", e)
            setToast({
                type: "failed",
                message: "Une erreur est survenue pendant la suppression.",
            })
        } finally {
            setBulkDeleting(false)
        }
    }

    useEffect(() => {
        fetchLives()
    }, [refreshKey, refreshKeyModify])

    return (
        <div className="my-8 ">
            {toast && (
                <Toast
                    type={toast.type}
                    message={toast.message}
                    onClose={() => setToast(null)}
                />
            )}
            {/* Barre d’actions groupées */}
            {selectedIds.size > 0 && (
                <div
                    className="mb-3 flex items-center justify-between rounded-md border border-[var(--border-main)] bg-[--background-four] px-3 py-2 text-[var(--text-primary)]"
                    role="status"
                    aria-live="polite"
                >
                    <span>
                        {selectedIds.size} live{selectedIds.size > 1 ? "s" : ""}{" "}
                        sélectionné{selectedIds.size > 1 ? "s" : ""}
                    </span>
                    <button
                        className="btn btn-danger"
                        onClick={handleBulkDelete}
                        disabled={bulkDeleting}
                        aria-disabled={bulkDeleting}
                    >
                        {bulkDeleting ? "Suppression…" : "Supprimer tout"}
                    </button>
                </div>
            )}

            <table className="backdrop-blur-md c-box-shadow-wrapper c-base-wrapper-component p-6 border-2 border-[var(--border-main)] rounded-[12px] w-full c-box-shadow-wrapper relative overflow-hidden">
                <thead>
                    <tr className="bg-[--background-four]">
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] w-10">
                            <input
                                ref={selectAllRef}
                                type="checkbox"
                                aria-label="Tout sélectionner"
                                checked={allSelected}
                                onChange={toggleSelectAll}
                            />
                        </th>
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] w-48">
                            Titre
                        </th>
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] w-32">
                            Visibilité
                        </th>

                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] w-48">
                            Utilisateur invité
                        </th>
                        {/* <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] w-32">
                            Début
                        </th> */}
                        {/* <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] w-32">
                            Fin
                        </th> */}
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-left text-[var(--text-primary)] w-48">
                            StreamKey
                        </th>
                        <th className="p-2 border-b-2 border-b-[#e0e0e0] text-center"></th>
                    </tr>
                </thead>
                <tbody>
                    {lives.map((live) => {
                        const id = live.apiVideoLiveStreamId
                        const checked = selectedIds.has(id)
                        return (
                            <tr
                                key={id}
                                className="border-b-2 border-solid border-[#eee] hover:bg-[--background-four] transition-all duration-200"
                                onClick={() =>
                                    navigate(`${ROOT_APP_URL}/live/${id}`)
                                }
                            >
                                <td className="p-2">
                                    <input
                                        type="checkbox"
                                        aria-label={`Sélectionner le live ${live.title}`}
                                        checked={checked}
                                        onClick={(e) => e.stopPropagation()}
                                        onChange={() => toggleSelect(id)}
                                    />
                                </td>
                                <td className="text-[var(--text-primary)] p-2">
                                    {live.title}
                                </td>
                                <td className="text-[var(--text-primary)] p-2">
                                    {live.isPublic ? "Public" : "Privé"}
                                </td>
                                <td className="text-[var(--text-primary)] p-2">
                                    {live.invitedUserId ?? ""}
                                </td>
                                {/* <td className="text-[var(--text-primary)] p-2">
                                    {formatDateTimeFR(live.startTime)}
                                </td>
                                <td className="text-[var(--text-primary)] p-2">
                                    {formatDateTimeFR(live.endTime)}
                                </td> */}

                                <td className="text-[var(--text-primary)] p-2">
                                    <div className="flex items-center gap-2">
                                        <span>
                                            {visibleFields[id]?.key
                                                ? live.streamKey ?? ""
                                                : "•••••••••••••"}
                                        </span>
                                        <button
                                            title={
                                                visibleFields[id]?.key
                                                    ? "Masquer"
                                                    : "Afficher"
                                            }
                                            onClick={(e) => {
                                                e.stopPropagation()
                                                toggleVisibility(id, "key")
                                            }}
                                            className="ml-2"
                                            style={{
                                                background: "none",
                                                border: "none",
                                                cursor: "pointer",
                                            }}
                                        >
                                            {visibleFields[id]?.key ? (
                                                <svg
                                                    xmlns="http://www.w3.org/2000/svg"
                                                    width="24"
                                                    height="24"
                                                    viewBox="0 0 24 24"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    strokeWidth="2"
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                >
                                                    <path d="M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49" />
                                                    <path d="M14.084 14.158a3 3 0 0 1-4.242-4.242" />
                                                    <path d="M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143" />
                                                    <path d="m2 2 20 20" />
                                                </svg>
                                            ) : (
                                                <svg
                                                    xmlns="http://www.w3.org/2000/svg"
                                                    width="24"
                                                    height="24"
                                                    viewBox="0 0 24 24"
                                                    fill="none"
                                                    stroke="currentColor"
                                                    strokeWidth="2"
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                >
                                                    <path d="M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0" />
                                                    <circle
                                                        cx="12"
                                                        cy="12"
                                                        r="3"
                                                    />
                                                </svg>
                                            )}
                                        </button>

                                        <div className="relative inline-block">
                                            <button
                                                title="Copier"
                                                onClick={(e) => {
                                                    e.stopPropagation()
                                                    handleCopy(
                                                        id + "_key",
                                                        live.streamKey ?? ""
                                                    )
                                                }}
                                                className="ml-2"
                                                style={{
                                                    background: "none",
                                                    border: "none",
                                                    cursor: "pointer",
                                                }}
                                            >
                                                {copied[id + "_key"] ? (
                                                    <svg
                                                        xmlns="http://www.w3.org/2000/svg"
                                                        width="24"
                                                        height="24"
                                                        viewBox="0 0 24 24"
                                                        fill="none"
                                                        stroke="currentColor"
                                                        strokeWidth="2"
                                                        strokeLinecap="round"
                                                        strokeLinejoin="round"
                                                    >
                                                        <rect
                                                            width="8"
                                                            height="4"
                                                            x="8"
                                                            y="2"
                                                            rx="1"
                                                            ry="1"
                                                        />
                                                        <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" />
                                                        <path d="m9 14 2 2 4-4" />
                                                    </svg>
                                                ) : (
                                                    <svg
                                                        xmlns="http://www.w3.org/2000/svg"
                                                        width="24"
                                                        height="24"
                                                        viewBox="0 0 24 24"
                                                        fill="none"
                                                        stroke="currentColor"
                                                        strokeWidth="2"
                                                        strokeLinecap="round"
                                                        strokeLinejoin="round"
                                                    >
                                                        <rect
                                                            width="8"
                                                            height="4"
                                                            x="8"
                                                            y="2"
                                                            rx="1"
                                                            ry="1"
                                                        />
                                                        <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" />
                                                    </svg>
                                                )}
                                            </button>
                                            {copied[id + "_key"] && (
                                                <div
                                                    role="status"
                                                    aria-live="polite"
                                                    className="absolute -top-8 left-1/2 -translate-x-1/2 whitespace-nowrap
                                     rounded px-2 py-1 text-xs text-white bg-black/80 shadow
                                     animate-[fadeIn_.12s_ease-out]"
                                                >
                                                    Texte copié !
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </td>

                                <td className="p-2 flex gap-2 justify-center">
                                    <button
                                        className="btn btn-base"
                                        onClick={(e) => {
                                            e.stopPropagation()
                                            handleModify(id)
                                        }}
                                    >
                                        Modifier
                                    </button>
                                    <button
                                        aria-label="Supprimer ce live"
                                        title="Supprimer"
                                        onClick={(e) => {
                                            e.stopPropagation()
                                            handleClickDeleteLive(id)
                                        }}
                                        className="rounded hover:bg-red-50 p-2"
                                        style={{
                                            background: "none",
                                            border: "none",
                                            cursor: "pointer",
                                            color: "var(--text-primary)",
                                        }}
                                    >
                                        <svg
                                            xmlns="http://www.w3.org/2000/svg"
                                            width="24"
                                            height="24"
                                            viewBox="0 0 24 24"
                                            fill="none"
                                            stroke="currentColor"
                                            strokeWidth="2"
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                        >
                                            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6" />
                                            <path d="M3 6h18" />
                                            <path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                                        </svg>
                                    </button>
                                </td>
                            </tr>
                        )
                    })}
                </tbody>
            </table>

            {/* Popover de modification */}
            <ProgrammedLiveModify
                isOpen={modifyOpen}
                liveId={editingId}
                initialTitle={editingTitle}
                initialIsPublic={editingIsPublic}
                initialThumbnailUrl={editingThumbnailUrl}
                onClose={() => setModifyOpen(false)}
                onLiveModified={handleLiveModified}
                initialInvitedUserId={editingInvitedUserId}
            />

            <style>{`
        @keyframes fadeIn {
          from { opacity: 0; transform: translate(-50%, -6px); }
          to   { opacity: 1; transform: translate(-50%, 0px); }
        }
      `}</style>
        </div>
    )
}

export default ProgrammedLive

