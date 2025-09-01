// src/hooks/useChat.ts
import { useEffect, useState, useCallback } from "react"
import * as signalR from "@microsoft/signalr"

interface ChatMessage {
  id: string
  senderId: number
  senderUsername: string
  content: string
  sentAt: string
  isDeleted?: boolean
}



export function useChat(liveId: string | undefined, userId: number | null, username: string | undefined) {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])


  const addSystemMessage = useCallback((text: string) => {
    setMessages(prev => [
      ...prev,
      {
        id: "",
        senderId: 0,
        senderUsername: "",
        content: text,
        sentAt: new Date().toISOString()
      }
    ])
  }, [])


  // Connexion au hub
  useEffect(() => {
    if (!liveId || !userId || !username) return

    const conn = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5268/livechat") // adapte l’URL
      .withAutomaticReconnect()
      .build()

    conn.on("ReceiveMessage", (msg: ChatMessage) => {
      setMessages((prev) => [...prev, msg])
    })

    conn.on("ReceiveSystemMessage", (msg: string) => {
      setMessages((prev) => [...prev, { id: "", senderId: 0, senderUsername: "", content: msg, sentAt: new Date().toISOString() }])
    })

    conn.on("MessageDeleted", (id: string) => {
      setMessages((prev) =>
        prev.map((m) => (m.id === id ? { ...m, isDeleted: true } : m))
      )
    })

    conn.on("UserTimeout", (targetId: number, duration: number) => {
      // console.log(`Utilisateur ${targetId} timeout pour ${duration} sec`)
    })

    conn.on("UserBanned", (targetId: number) => {
      // console.log(`Utilisateur ${targetId} banni`)
      
    })

    conn
      .start()
      .then(() => {
        conn.invoke("JoinLive", liveId, userId)
        
      })
      .catch((err) => console.error("Erreur connexion SignalR:", err))

    setConnection(conn)
    
    return () => {
      conn.invoke("LeaveLive", liveId, userId)
      conn.stop()
      
    }
  }, [liveId, userId, username])

  // Envoyer un message
  const sendMessage = useCallback(
    async (content: string) => {
      if (!connection || !liveId || !userId || !username) return
      try {
        await connection.invoke("SendMessage", liveId, userId, username, content)
      } catch (err) {
        console.error("Erreur envoi message:", err)
      }
    },
    [connection, liveId, userId, username]
  )

  const handleReceiveSystemMessage = (msg: string) => {
      // IMPORTANT: ce message ne doit être envoyé que via Clients.Caller côté serveur
      // => ainsi, seul l'auteur de l'action le voit.
      addSystemMessage(msg)
    }

  // Actions de modération
  const deleteMessage = (messageId: string, userId: number, liveId: string) => {
    if (!connection || !liveId) return
    connection.invoke("DeleteMessage", messageId, userId, liveId)
  }

  const banUser = (targetUserId: number, targetUsername: string) => {
    if (!connection || !liveId || !userId) return
    connection.invoke("BanUser", liveId, userId, targetUserId, targetUsername)

  }

  const timeoutUser = (targetUserId: number, targetUsername: string, duration: number) => {
    if (!connection || !liveId || !userId) return
    connection.invoke("TimeoutUser", liveId, userId, targetUserId, targetUsername, duration)
  }

  return { messages, sendMessage, deleteMessage, banUser, timeoutUser }
}
