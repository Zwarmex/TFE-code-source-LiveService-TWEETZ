// ProgrammedLiveModify.tsx
import React, { useEffect, useMemo, useRef, useState } from "react"
import { liveAPI } from "@/services/config"
import ReactDOM from "react-dom"
import { ProgrammedLiveModifyProps } from "../types/LiveTypes"

const ProgrammedLiveModify: React.FC<ProgrammedLiveModifyProps> = ({
    isOpen,
    liveId,
    initialTitle = "",
    initialIsPublic,
    initialThumbnailUrl = null,
    initialInvitedUserId = null,
    onClose,
    onSaved,
    onLiveModified,
}) => {
    const [title, setTitle] = useState(initialTitle)
    const [isPublic, setIsPublic] = useState(initialIsPublic)
    const [thumbnailFile, setThumbnailFile] = useState<File | null>(null)
    const [initialThumbnailFile, setInitialThumbnailFile] = useState<
        string | null
    >(initialThumbnailUrl)
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const fileInputRef = useRef<HTMLInputElement | null>(null)
    const panelRef = useRef<HTMLDivElement | null>(null)
    const cancelBtnRef = useRef<HTMLButtonElement | null>(null)
    const [invitedUserId, setInvitedUserId] = useState<number | null>(null)

    const canSave = useMemo(() => {
        return !!liveId && title.trim().length > 0
    }, [liveId, title])

    useEffect(() => {
        if (isOpen) {
            setTitle(initialTitle)
            setIsPublic(initialIsPublic)
            setThumbnailFile(null)
            setInitialThumbnailFile(initialThumbnailUrl ?? null)
            setInvitedUserId(initialInvitedUserId ?? null)
            setError(null)

            setTimeout(() => cancelBtnRef.current?.focus(), 0)
        }
    }, [isOpen, initialTitle, initialIsPublic, initialThumbnailUrl])

    // Fermer avec ESC
    useEffect(() => {
        if (!isOpen) return
        const onKey = (e: KeyboardEvent) => {
            if (e.key === "Escape") onClose()
        }
        document.addEventListener("keydown", onKey)
        return () => document.removeEventListener("keydown", onKey)
    }, [isOpen, onClose])

    // Fermer si clic en dehors
    useEffect(() => {
        if (!isOpen) return
        const handler = (e: MouseEvent) => {
            if (
                panelRef.current &&
                !panelRef.current.contains(e.target as Node)
            ) {
                onClose()
            }
        }
        document.addEventListener("mousedown", handler)
        return () => document.removeEventListener("mousedown", handler)
    }, [isOpen, onClose])

    // Cleanup URL blob
    useEffect(() => {
        return () => {
            if (initialThumbnailFile?.startsWith("blob:"))
                URL.revokeObjectURL(initialThumbnailFile)
        }
    }, [initialThumbnailFile])

    const pickImage = () => fileInputRef.current?.click()

    const handleFileChange = (file: File | null) => {
        setThumbnailFile(file)
        if (initialThumbnailFile?.startsWith("blob:"))
            URL.revokeObjectURL(initialThumbnailFile)
        if (file) {
            const url = URL.createObjectURL(file)
            setInitialThumbnailFile(url)
        } else {
            setInitialThumbnailFile(initialThumbnailUrl ?? null)
        }
    }

    const handleSave = async () => {
        if (!canSave || !liveId) return
        setError(null)
        setSaving(true)
        try {
            // 1) PATCH: titre et visibilité
            const metaRes = await fetch(`${liveAPI}/${liveId}`, {
                method: "PATCH",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ title, isPublic, invitedUserId }),
            })
            if (!metaRes.ok) {
                const t = await metaRes.text()

                throw new Error("Erreur de mise à jour des infos: " + t)
            }

            // 2) Upload miniature si nouvelle image
            var newThumbUrl: string | null | undefined = undefined
            if (thumbnailFile) {
                const fd = new FormData()
                fd.append("file", thumbnailFile)
                const up = await fetch(`${liveAPI}/${liveId}/thumbnail`, {
                    method: "POST",
                    body: fd,
                    credentials: "include",
                })
                if (!up.ok) {
                    const t = await up.text()
                    throw new Error("Upload miniature échoué: " + t)
                }
            }

            onSaved?.({
                title,
                isPublic: initialIsPublic ?? true,
                thumbnailUrl: newThumbUrl ?? initialThumbnailUrl ?? null,
                invitedUserId: invitedUserId ?? null,
            })
            onLiveModified?.()
            onClose()
        } catch (e: any) {
            console.error(e)
            setError(e?.message ?? "Une erreur est survenue")
        } finally {
            setSaving(false)
        }
    }

    if (!isOpen) return null

    return ReactDOM.createPortal(
        <div
            aria-modal="true"
            role="dialog"
            aria-labelledby="programmed-live-modify-title"
            className="fixed inset-0 z-[1001] flex items-center justify-center"
        >
            {/* Backdrop */}
            <div className="absolute inset-0 bg-black/40 backdrop-blur-[2px]" />

            {/* Popover panel */}
            <div
                ref={panelRef}
                className="relative z-[1002] w-[92vw]  max-w-md rounded-xl border-2 border-[var(--border-main)] p-6 shadow-2xl c-base-wrapper-component"
            >
                <h3
                    id="programmed-live-modify-title"
                    className="c-h2-title mb-4"
                >
                    Modifier le live
                </h3>

                {/* Image cliquable */}
                <div className="mb-4">
                    <label className="text-[var(--text-primary)] block mb-2">
                        Miniature
                    </label>
                    <div
                        onClick={pickImage}
                        className="relative aspect-video w-full cursor-pointer overflow-hidden rounded-lg border border-[var(--border-main)] bg-[var(--bg-muted)]"
                        title="Cliquez pour changer l'image"
                        tabIndex={0}
                        onKeyDown={(e) =>
                            (e.key === "Enter" || e.key === " ") && pickImage()
                        }
                    >
                        {initialThumbnailFile ? (
                            // eslint-disable-next-line @next/next/no-img-element
                            <img
                                src={initialThumbnailFile}
                                alt="Miniature du live"
                                className="h-full w-full object-cover"
                            />
                        ) : (
                            <div className="flex h-full w-full items-center justify-center text-[var(--text-secondary)]">
                                Cliquez pour choisir une image
                            </div>
                        )}
                    </div>
                    <input
                        ref={fileInputRef}
                        type="file"
                        accept="image/*"
                        className="hidden"
                        onChange={(e) =>
                            handleFileChange(e.target.files?.[0] ?? null)
                        }
                    />
                </div>
                {initialThumbnailFile && (
                    <button
                        type="button"
                        className="mt-2 btn text-[var(--text-primary)] w-full"
                        onClick={async () => {
                            if (!liveId) return
                            if (
                                window.confirm(
                                    "Êtes-vous sûr de vouloir supprimer la miniature ?"
                                )
                            ) {
                                setSaving(true)
                                setError(null)
                                try {
                                    const res = await fetch(
                                        `${liveAPI}/${liveId}/thumbnail`,
                                        {
                                            method: "DELETE",
                                            credentials: "include",
                                        }
                                    )
                                    if (!res.ok) {
                                        const t = await res.text()
                                        throw new Error(
                                            "Suppression échouée: " + t
                                        )
                                    }
                                    setThumbnailFile(null)
                                    setInitialThumbnailFile(null)
                                } catch (e: any) {
                                    setError(
                                        e?.message ??
                                            "Erreur lors de la suppression"
                                    )
                                } finally {
                                    setSaving(false)
                                }
                            }
                        }}
                        disabled={saving}
                    >
                        Supprimer la miniature
                    </button>
                )}
                {/* Titre */}
                <div className="mb-4">
                    <label
                        htmlFor="plm-title"
                        className="text-[var(--text-primary)] block mb-2"
                    >
                        Titre
                    </label>
                    <input
                        id="plm-title"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        className="c-simple-input c-simple-input--rounded w-full"
                        placeholder="Titre du live"
                    />
                </div>

                {/* Visibilité */}
                <div className="mb-2">
                    <label
                        htmlFor="plm-visibility"
                        className="text-[var(--text-primary)] block mb-2"
                    >
                        Visibilité
                    </label>
                    <select
                        id="plm-visibility"
                        value={isPublic ? "public" : "private"}
                        onChange={(e) =>
                            setIsPublic(e.target.value === "public")
                        }
                        className="c-simple-input c-simple-input--rounded w-full"
                    >
                        <option value="public">Public</option>
                        <option value="private">Privé</option>
                    </select>
                    {!isPublic && (
                        <div>
                            <label
                                htmlFor="invitedUserId"
                                className="text-[var(--text-primary)] block mb-2"
                            >
                                ID utilisateur invité
                            </label>
                            <input
                                required
                                id="invitedUserId"
                                type="number"
                                value={invitedUserId ?? ""} // <- on lit le state
                                onChange={(e) => {
                                    const v = e.target.value
                                    setInvitedUserId(Number(v))
                                }}
                                placeholder="ID de l'utilisateur invité"
                                className="c-simple-input c-simple-input--rounded"
                            />
                        </div>
                    )}
                </div>

                {error && (
                    <div
                        role="alert"
                        className="mt-2 rounded-md border border-red-500 bg-red-50 px-3 py-2 text-sm text-red-700"
                    >
                        {error}
                    </div>
                )}

                {/* Actions */}
                <div className="mt-6 flex items-center justify-end gap-3">
                    <button
                        ref={cancelBtnRef}
                        type="button"
                        className="btn text-[var(--text-primary)]"
                        onClick={onClose}
                        disabled={saving}
                    >
                        Annuler
                    </button>
                    <button
                        type="button"
                        className="btn btn-base"
                        onClick={handleSave}
                        disabled={!canSave || saving}
                        aria-disabled={!canSave || saving}
                    >
                        {saving ? "Enregistrement…" : "Enregistrer"}
                    </button>
                </div>
            </div>
        </div>,
        document.getElementById("portal") as HTMLElement
    )
}

export default ProgrammedLiveModify

